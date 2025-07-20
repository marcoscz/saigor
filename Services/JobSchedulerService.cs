using Quartz;
using Quartz.Spi;
using Saigor.Models;
using Saigor.Repositories;
using System.Diagnostics;
using Saigor.Shared;
using Saigor.Utils;

namespace Saigor.Services
{
    public class JobSchedulerService(
        ISchedulerFactory schedulerFactory,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<JobSchedulerService> logger) : IJobSchedulerService, IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly ILogger<JobSchedulerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private IScheduler? _scheduler;

        /// <summary>
        /// Inicia o job especificado pelo nome.
        /// </summary>
        /// <param name="jobName">Nome do job a ser iniciado.</param>
        /// <returns>True se o job foi agendado com sucesso, false caso contrário.</returns>
        public async Task<bool> StartJobAsync(string jobName)
        {
            if (!ErrorHandlingHelper.ValidateStringParameter(jobName, "jobName", _logger))
                return false;

            if (!ErrorHandlingHelper.ValidateNotNullParameter(_scheduler, "scheduler", _logger))
                return false;

            return await ScopeHelper.ExecuteInScopeAsync(
                _scopeFactory,
                async (serviceProvider) =>
                {
                    _logger.LogInformation("Tentando iniciar job {JobName}", jobName);

                    var jobRepository = serviceProvider.GetRequiredService<IJobRepository>();
                    var job = await jobRepository.GetByNameAsync(jobName);

                    if (job == null)
                    {
                        _logger.LogWarning("Job {JobName} não encontrado no banco de dados", jobName);
                        return false;
                    }

                    var jobKey = new JobKey(jobName);
                    var jobExists = await _scheduler!.CheckExists(jobKey);

                    if (jobExists)
                    {
                        // Se o job já existe, resuma a execução
                        await _scheduler.ResumeJob(jobKey);
                        _logger.LogInformation("Job {JobName} resumido", jobName);
                    }
                    else
                    {
                        // Se o job não existe, agende-o
                        await ScheduleJobAsync(job);
                        _logger.LogInformation("Job {JobName} agendado", jobName);
                    }

                    // Atualiza o status no banco
                    await UpdateJobStatusAsync(jobName, JobStatus.Rodando);
                    _logger.LogInformation("Job {JobName} iniciado com sucesso", jobName);
                    return true;
                },
                _logger,
                $"Erro ao iniciar job {jobName}",
                $"Iniciar job {jobName}"
            );
        }

        private async Task<bool> UpdateJobStatusAsync(string jobName, JobStatus status)
        {
            return await ScopeHelper.ExecuteInScopeAsync(
                _scopeFactory,
                async (serviceProvider) =>
                {
                    var jobRepository = serviceProvider.GetRequiredService<IJobRepository>();
                    var job = await jobRepository.GetByNameAsync(jobName);
                    if (job != null)
                    {
                        job.Status = status;
                        await jobRepository.UpdateAsync(job);
                        _logger.LogInformation("Status do job {JobName} atualizado para {Status}", jobName, status);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Job {JobName} não encontrado no banco de dados", jobName);
                        return false;
                    }
                },
                _logger,
                $"Erro ao atualizar status do job {jobName}",
                $"Atualizar status do job {jobName}"
            );
        }

        public async Task<bool> StopJobAsync(string jobName)
        {
            if (!ErrorHandlingHelper.ValidateStringParameter(jobName, "jobName", _logger))
                return false;

            if (!ErrorHandlingHelper.ValidateNotNullParameter(_scheduler, "scheduler", _logger))
                return false;

            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    _logger.LogInformation("Tentando parar job {JobName}", jobName);

                    var jobKey = new JobKey(jobName);
                    var jobExists = await _scheduler!.CheckExists(jobKey);

                    _logger.LogInformation("Job {JobName} existe no scheduler: {Exists}", jobName, jobExists);

                    if (jobExists)
                    {
                        await PauseAndRemoveJobAsync(jobKey, jobName);
                        await UpdateJobStatusAsync(jobName, JobStatus.Parado);
                        _logger.LogInformation("Job {JobName} parado com sucesso", jobName);
                        return true;
                    }

                    _logger.LogWarning("Job {JobName} não encontrado no scheduler", jobName);
                    // Mesmo que não esteja no scheduler, atualiza o status no banco
                    var updated = await UpdateJobStatusAsync(jobName, JobStatus.Parado);
                    if (updated)
                    {
                        _logger.LogInformation("Status do job {JobName} atualizado para Parado (não estava no scheduler)", jobName);
                        return true;
                    }
                    return false;
                },
                _logger,
                $"Erro ao parar job {jobName}",
                $"Parar job {jobName}"
            );
        }

        private async Task PauseAndRemoveJobAsync(JobKey jobKey, string jobName)
        {
            if (_scheduler == null)
            {
                _logger.LogWarning("Scheduler não está inicializado ao tentar pausar/remover job {JobName}", jobName);
                return;
            }
            // Pausa o job primeiro
            await _scheduler.PauseJob(jobKey);
            _logger.LogInformation("Job {JobName} pausado", jobName);

            // Remove o job do scheduler
            var deleted = await _scheduler.DeleteJob(jobKey);
            _logger.LogInformation("Job {JobName} removido do scheduler: {Deleted}", jobName, deleted);
        }

        /// <summary>
        /// Obtém o status atual de um job.
        /// </summary>
        /// <param name="jobName">Nome do job.</param>
        /// <returns>Status do job ou null se não encontrado.</returns>
        public async Task<string?> GetJobStatusAsync(string jobName)
        {
            return await ScopeHelper.ExecuteInScopeAsync(
                _scopeFactory,
                async (serviceProvider) =>
                {
                    var jobRepository = serviceProvider.GetRequiredService<IJobRepository>();
                    var job = await jobRepository.GetByNameAsync(jobName);
                    return job?.Status.ToString();
                },
                _logger,
                null,
                $"Erro ao obter status do job {jobName}",
                $"Obter status do job {jobName}"
            );
        }

        /// <summary>
        /// Inicia o serviço de agendamento e agenda todos os jobs existentes.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null && !_scheduler.IsShutdown)
                return;

            try
            {
                _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
                _scheduler.JobFactory = new JobFactory(_serviceProvider);
                await _scheduler.Start(cancellationToken);

                using var scope = _scopeFactory.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                var jobs = await jobRepository.GetAllAsync();

                foreach (var job in jobs)
                {
                    await ScheduleJobAsync(job, cancellationToken);
                }

                _logger.LogInformation("Serviço de agendamento iniciado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar serviço de agendamento");
                throw;
            }
        }

        /// <summary>
        /// Agenda um job individual.
        /// </summary>
        private async Task ScheduleJobAsync(JobModel job, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(job.Name))
                {
                    _logger.LogWarning("Job com ID {JobId} não possui um nome válido", job.Id);
                    return;
                }

                var jobKey = new JobKey(job.Name);
                if (await _scheduler!.CheckExists(jobKey, cancellationToken))
                {
                    _logger.LogInformation("Job {JobName} já está agendado", job.Name);
                    return;
                }

                if (string.IsNullOrWhiteSpace(job.Schedule))
                {
                    _logger.LogWarning("Job {JobName} não possui uma expressão CRON válida", job.Name);
                    return;
                }

                // Valida a expressão CRON
                CronScheduleBuilder.CronSchedule(job.Schedule);

                var jobDetail = JobBuilder.Create<ExecuteJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData("JobId", job.Id)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithCronSchedule(job.Schedule)
                    .Build();

                await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                job.Status = JobStatus.Rodando;

                using var scope = _scopeFactory.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                await jobRepository.UpdateAsync(job);

                _logger.LogInformation("Job {JobName} agendado com sucesso", job.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao agendar job {JobName}", job.Name);
                job.Status = JobStatus.Falhou;

                using var scope = _scopeFactory.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                await jobRepository.UpdateAsync(job);
            }
        }

        public async Task ScheduleJobAsync(JobModel job)
        {
            await ScheduleJobAsync(job, CancellationToken.None);
        }

        /// <summary>
        /// Para o serviço de agendamento.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown(cancellationToken);
                _logger.LogInformation("Serviço de agendamento parado");
            }
        }

        public async Task UnscheduleJobAsync(int jobId)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var job = await jobRepository.GetByIdAsync(jobId);
            if (job == null || string.IsNullOrWhiteSpace(job.Name))
            {
                _logger.LogWarning("Job com ID {JobId} não encontrado ou sem nome válido", jobId);
                return;
            }
            if (_scheduler == null)
            {
                _logger.LogWarning("Scheduler não está inicializado");
                return;
            }
            var jobKey = new Quartz.JobKey(job.Name);
            await _scheduler.DeleteJob(jobKey);
            _logger.LogInformation("Job {JobName} removido do scheduler", job.Name);
        }

        public async Task<bool> IsJobScheduledAsync(int jobId)
        {
            using var scope = _scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var job = await jobRepository.GetByIdAsync(jobId);
            if (job == null || string.IsNullOrWhiteSpace(job.Name) || _scheduler == null)
            {
                return false;
            }
            var jobKey = new Quartz.JobKey(job.Name);
            return await _scheduler.CheckExists(jobKey);
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

    public class ScopedJobWrapper(IJob innerJob, IServiceScope scope) : IJob
    {
        public IJob InnerJob { get; } = innerJob;
        public IServiceScope Scope { get; } = scope;

        public Task Execute(IJobExecutionContext context)
        {
            return InnerJob.Execute(context);
        }
    }

    [DisallowConcurrentExecution]
    public class ExecuteJob(IServiceScopeFactory scopeFactory, ILogger<ExecuteJob> logger) : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly ILogger<ExecuteJob> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.JobDetail.JobDataMap.GetInt("JobId");
            var startTime = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();

            var job = await jobRepository.GetByIdAsync(jobId);

            if (job == null)
            {
                _logger.LogError("Job com ID {JobId} não encontrado", jobId);
                return;
            }

            _logger.LogInformation("Executando job {JobName}", job.Name);

            try
            {
                job.Status = JobStatus.Rodando;
                job.LastExecution = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);

                var output = await CommandExecutor.RunCommandAsync(job.Command);
                var executionTime = DateTime.UtcNow - startTime;

                job.Status = JobStatus.Completado;
                job.LastExecution = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);

                // Salvar log de execução
                var log = new LogModel
                {
                    JobId = job.Id,
                    ExecutionTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    Status = "Completado",
                    Output = output.Length > 1000 ? output[..1000] + "..." : output
                };

                await logRepository.AddAsync(log);
                await logRepository.SaveChangesAsync();

                _logger.LogInformation("Job {JobName} executado com sucesso em {ExecutionTime}ms",
                    job.Name, executionTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                job.Status = JobStatus.Falhou;
                await jobRepository.UpdateAsync(job);

                var log = new LogModel
                {
                    JobId = job.Id,
                    ExecutionTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    Status = "Falhou",
                    Output = ex.Message
                };

                await logRepository.AddAsync(log);
                await logRepository.SaveChangesAsync();

                _logger.LogError(ex, "Erro ao executar job {JobName} após {ExecutionTime}ms",
                    job.Name, executionTime.TotalMilliseconds);
            }
        }
    }
}

