using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Quartz;
using Saigor.Data;
using Saigor.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Logging -------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ------------------- Database -------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ------------------- Blazor e UI -------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(); // MudBlazor

// ------------------- Quartz.NET -------------------
builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
    options.AwaitApplicationStarted = true;
});

// ------------------- Application Services -------------------
builder.Services.AddTransient<ExecuteJob>();
builder.Services.AddSingleton<JobSchedulerService>();
// Não registre como HostedService se já está como Singleton e gerencia internamente
// builder.Services.AddHostedService(provider => provider.GetRequiredService<JobSchedulerService>());

var app = builder.Build();

// ------------------- Error Handling -------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ------------------- Security Headers -------------------
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

// ------------------- Middleware Pipeline -------------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ------------------- Endpoints -------------------
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
