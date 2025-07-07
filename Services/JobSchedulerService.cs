using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Spi;
using Saigor.Data;
using Saigor.Models;
using System.Diagnostics;

namespace Saigor.Services
{
    public class JobSchedulerService(ISchedulerFactory schedulerFactory, IServiceProvider serviceProvider, ILogger<JobSchedulerService> logger) : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ILogger<JobSchedulerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private IScheduler? _scheduler;

        /// <summary>
        /// Inicia o job especificado pelo nome.
        /// </summary>
        /// <param name="jobName">Nome do job a ser iniciado.</param>
        /// <returns>True se o job foi agendado com sucesso, false caso contrário.</returns>
        public async Task<bool> StartJobAsync(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                return false;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var job = await db.Jobs.FirstOrDefaultAsync(j => j.Name == jobName);

            if (job == null || string.IsNullOrWhiteSpace(job.Schedule))
                return false;

            var scheduler = _scheduler ??= await _schedulerFactory.GetScheduler();
            scheduler.JobFactory = new JobFactory(_serviceProvider);

            var jobKey = new JobKey(job.Name);

            if (await scheduler.CheckExists(jobKey))
                return false; // já agendado

            try
            {
                CronScheduleBuilder.CronSchedule(job.Schedule); // valida cron

                var jobDetail = JobBuilder.Create<ExecuteJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData("JobId", job.Id)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithCronSchedule(job.Schedule)
                    .Build();

                await scheduler.ScheduleJob(jobDetail, trigger);
                job.Status = JobStatus.Rodando;
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao agendar job {JobName}", jobName);
                return false;
            }
        }

        /// <summary>
        /// Para o job especificado pelo nome.
        /// </summary>
        /// <param name="jobName">Nome do job a ser parado.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>True se o job foi parado com sucesso, false caso contrário.</returns>
        public async Task<bool> StopJobAsync(string jobName, CancellationToken cancellationToken = default)
        {
            if (_scheduler == null)
                return false;

            var jobKey = new JobKey(jobName);
            if (await _scheduler.CheckExists(jobKey, cancellationToken))
            {
                await _scheduler.DeleteJob(jobKey, cancellationToken);
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var job = await db.Jobs.FirstOrDefaultAsync(j => j.Name == jobName, cancellationToken);
                if (job != null)
                {
                    job.Status = JobStatus.Parado;
                    await db.SaveChangesAsync(cancellationToken);
                }
                _logger.LogInformation("Job {JobName} foi parado com sucesso.", jobName);
                return true;
            }

            _logger.LogWarning("Job {JobName} não encontrado.", jobName);
            return false;
        }

        /// <summary>
        /// Inicia o serviço de agendamento e agenda todos os jobs existentes.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null && !_scheduler.IsShutdown)
                return;

            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = new JobFactory(_serviceProvider);
            await _scheduler.Start(cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var jobs = await db.Jobs.ToListAsync(cancellationToken);

            foreach (var job in jobs)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(job.Name))
                    {
                        _logger.LogWarning("Job com ID {JobId} não possui um nome válido.", job.Id);
                        continue;
                    }
                    var jobKey = new JobKey(job.Name);
                    if (await _scheduler.CheckExists(jobKey, cancellationToken))
                    {
                        _logger.LogInformation("Job {JobName} já está agendado.", job.Name);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(job.Schedule))
                    {
                        _logger.LogWarning("Job {JobName} não possui uma expressão CRON válida.", job.Name);
                        continue;
                    }

                    // Valida a expressão CRON
                    try
                    {
                        CronScheduleBuilder.CronSchedule(job.Schedule);
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError("CRON inválida para o job {JobName}: {Schedule}", job.Name ?? ex.Message, job.Schedule ?? "sem cron");
                        continue;
                    }

                    var jobDetail = JobBuilder.Create<ExecuteJob>()
                        .WithIdentity(jobKey)
                        .UsingJobData("JobId", job.Id)
                        .Build();

                    var trigger = TriggerBuilder.Create()
                        .WithCronSchedule(job.Schedule)
                        .Build();

                    await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                    job.Status = JobStatus.Rodando;
                    _logger.LogInformation("Job {JobName} agendado com sucesso.", job.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao agendar o job {JobName}.", job.Name);
                    job.Status = JobStatus.Falhou;
                }
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Para o serviço de agendamento.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null)
                await _scheduler.Shutdown(cancellationToken);
        }
    }

    public class JobFactory(IServiceProvider serviceProvider) : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            return new ScopedJobWrapper(job!, scope);
        }

        public void ReturnJob(IJob job)
        {
            if (job is ScopedJobWrapper wrapper)
            {
                wrapper.Scope.Dispose();
            }
        }
    }

    public class ScopedJobWrapper : IJob
    {
        public IJob InnerJob { get; }
        public IServiceScope Scope { get; }

        public ScopedJobWrapper(IJob innerJob, IServiceScope scope)
        {
            InnerJob = innerJob;
            Scope = scope;
        }

        public Task Execute(IJobExecutionContext context)
        {
            return InnerJob.Execute(context);
        }
    }

    [DisallowConcurrentExecution]
    public class ExecuteJob : IJob
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ExecuteJob> _logger;

        public ExecuteJob(ApplicationDbContext db, ILogger<ExecuteJob> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.JobDetail.JobDataMap.GetInt("JobId");
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);

            if (job != null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(job.Command))
                    {
                        _logger.LogWarning("Job {JobName} não possui um comando válido.", job.Name);
                        return;
                    }

                    job.Status = JobStatus.Rodando;
                    await _db.SaveChangesAsync();

                    var output = await CommandExecutor.RunCommandAsync(job.Command);

                    job.LastExecution = DateTime.Now;
                    job.Status = JobStatus.Completado;

                    _db.Logs.Add(new Log
                    {
                        JobId = job.Id,
                        ExecutionTime = DateTime.Now,
                        Output = output,
                        Status = "Sucesso"
                    });

                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Job {JobName} executado com sucesso.", job.Name);
                }
                catch (Exception ex)
                {
                    job.Status = JobStatus.Falhou;
                    _db.Logs.Add(new Log
                    {
                        JobId = job.Id,
                        ExecutionTime = DateTime.Now,
                        Output = ex.Message,
                        Status = "Erro"
                    });

                    await _db.SaveChangesAsync();
                    _logger.LogError(ex, "Erro ao executar o job {JobName}.", job.Name);
                }
            }
        }
    }

    public static class CommandExecutor
    {
        public static async Task<string> RunCommandAsync(string command)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            var exited = await Task.Run(() => process.WaitForExit(10000)); // 10 segundos
            if (!exited)
            {
                process.Kill();
                return "Erro: Tempo limite excedido.";
            }

            return string.IsNullOrWhiteSpace(error) ? output : $"Erro: {error}";
        }
    }
}
