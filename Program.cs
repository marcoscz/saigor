using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Quartz;
using Saigor.Data;
using Saigor.Models;
using Saigor.Repositories;
using Saigor.Repositories.Base;
using Saigor.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Culture e Timezone -------------------
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

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
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IRepository<JobModel>, JobRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IConexaoRepository, ConexaoRepository>();
builder.Services.AddScoped<IRepository<ConexaoModel>, ConexaoRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<IRepository<TarefaModel>, TarefaRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IJobTarefaRepository, JobTarefaRepository>();
builder.Services.AddScoped<IRepository<JobTarefaModel>, JobTarefaRepository>();

// ------------------- CRUD Services -------------------
builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
builder.Services.AddScoped<ICrudService<JobTarefaModel>, CrudService<JobTarefaModel>>();
builder.Services.AddScoped<ICrudService<TarefaModel>, CrudService<TarefaModel>>();
builder.Services.AddScoped<ICrudService<JobModel>, CrudService<JobModel>>();
builder.Services.AddScoped<ICrudService<ConexaoModel>, CrudService<ConexaoModel>>();

// ------------------- Validation Services -------------------
builder.Services.AddScoped<IValidationService, ValidationService>();

// ------------------- Cache Services -------------------
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// ------------------- Other Services -------------------
builder.Services.AddHttpClient<ExternalApiService>();
builder.Services.AddScoped<ICommandExecutor, CommandExecutorService>();
builder.Services.AddScoped<HealthCheckService>();
builder.Services.AddScoped<ExecuteJob>();

// ------------------- HTTP Client -------------------
builder.Services.AddHttpClient();

// Registrar JobSchedulerService como Singleton para IHostedService e IJobSchedulerService
builder.Services.AddSingleton<JobSchedulerService>();
builder.Services.AddHostedService<JobSchedulerService>(provider => provider.GetRequiredService<JobSchedulerService>());
builder.Services.AddSingleton<IJobSchedulerService>(provider => provider.GetRequiredService<JobSchedulerService>());

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
