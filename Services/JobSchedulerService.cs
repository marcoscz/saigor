using Quartz;
using Quartz.Spi;
using Saigor.Models;
using System.Diagnostics;

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
            if (string.IsNullOrWhiteSpace(jobName))
            {
                _logger.LogWarning("Nome do job não pode ser vazio");
                return false;
            }

            try
            {
                _logger.LogInformation("Tentando iniciar job {JobName}", jobName);

                using var scope = _scopeFactory.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                var job = await jobRepository.GetByNameAsync(jobName);
                if (job == null)
                {
                    _logger.LogWarning("Job {JobName} não encontrado no banco de dados", jobName);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(job.Schedule))
                {
                    _logger.LogWarning("Job {JobName} não possui agendamento válido", jobName);
                    return false;
                }

                var scheduler = _scheduler ??= await _schedulerFactory.GetScheduler();
                scheduler.JobFactory = new JobFactory(_serviceProvider);

                var jobKey = new JobKey(job.Name);
                var jobExists = await scheduler.CheckExists(jobKey);

                _logger.LogInformation("Job {JobName} já existe no scheduler: {Exists}", jobName, jobExists);

                if (jobExists)
                {
                    _logger.LogInformation("Job {JobName} já está agendado, removendo primeiro", jobName);
                    await scheduler.DeleteJob(jobKey);
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

                await scheduler.ScheduleJob(jobDetail, trigger);

                job.Status = JobStatus.Rodando;
                await jobRepository.UpdateAsync(job);

                _logger.LogInformation("Job {JobName} iniciado com sucesso", jobName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar job {JobName}", jobName);
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
            if (string.IsNullOrWhiteSpace(jobName))
            {
                _logger.LogWarning("Nome do job não pode ser vazio");
                return false;
            }

            if (_scheduler == null)
            {
                _logger.LogWarning("Scheduler não está inicializado");
                return false;
            }

            try
            {
                _logger.LogInformation("Tentando parar job {JobName}", jobName);

                var jobKey = new JobKey(jobName);
                var jobExists = await _scheduler.CheckExists(jobKey, cancellationToken);

                _logger.LogInformation("Job {JobName} existe no scheduler: {Exists}", jobName, jobExists);

                if (jobExists)
                {
                    // Pausa o job primeiro
                    await _scheduler.PauseJob(jobKey, cancellationToken);
                    _logger.LogInformation("Job {JobName} pausado", jobName);

                    // Remove o job do scheduler
                    var deleted = await _scheduler.DeleteJob(jobKey, cancellationToken);
                    _logger.LogInformation("Job {JobName} removido do scheduler: {Deleted}", jobName, deleted);

                    // Atualiza o status no banco de dados
                    using var scope = _scopeFactory.CreateScope();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                    var job = await jobRepository.GetByNameAsync(jobName);
                    if (job != null)
                    {
                        job.Status = JobStatus.Parado;
                        await jobRepository.UpdateAsync(job);
                        _logger.LogInformation("Status do job {JobName} atualizado para Parado", jobName);
                    }
                    else
                    {
                        _logger.LogWarning("Job {JobName} não encontrado no banco de dados", jobName);
                    }

                    _logger.LogInformation("Job {JobName} parado com sucesso", jobName);
                    return true;
                }

                _logger.LogWarning("Job {JobName} não encontrado no scheduler", jobName);

                // Mesmo que não esteja no scheduler, atualiza o status no banco
                using var scope2 = _scopeFactory.CreateScope();
                var jobRepository2 = scope2.ServiceProvider.GetRequiredService<IJobRepository>();
                var job2 = await jobRepository2.GetByNameAsync(jobName);
                if (job2 != null)
                {
                    job2.Status = JobStatus.Parado;
                    await jobRepository2.UpdateAsync(job2);
                    _logger.LogInformation("Status do job {JobName} atualizado para Parado (não estava no scheduler)", jobName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar job {JobName}", jobName);
                return false;
            }
        }

        /// <summary>
        /// Obtém o status atual de um job.
        /// </summary>
        /// <param name="jobName">Nome do job.</param>
        /// <returns>Status do job ou null se não encontrado.</returns>
        public async Task<string?> GetJobStatusAsync(string jobName)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                var job = await jobRepository.GetByNameAsync(jobName);
                return job?.Status.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status do job {JobName}", jobName);
                return null;
            }
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
        private async Task ScheduleJobAsync(Job job, CancellationToken cancellationToken)
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
                job.LastExecution = startTime.ToUniversalTime();
                await jobRepository.UpdateAsync(job);

                var output = await CommandExecutor.RunCommandAsync(job.Command);
                var executionTime = DateTime.UtcNow - startTime;

                job.Status = JobStatus.Completado;
                await jobRepository.UpdateAsync(job);

                // Salvar log de execução
                var log = new Log
                {
                    JobId = job.Id,
                    ExecutionTime = startTime.ToUniversalTime(),
                    Status = "Completado",
                    Output = output.Length > 1000 ? output[..1000] + "..." : output
                };

                await logRepository.AddAsync(log);

                _logger.LogInformation("Job {JobName} executado com sucesso em {ExecutionTime}ms",
                    job.Name, executionTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                job.Status = JobStatus.Falhou;
                await jobRepository.UpdateAsync(job);

                var log = new Log
                {
                    JobId = job.Id,
                    ExecutionTime = startTime.ToUniversalTime(),
                    Status = "Falhou",
                    Output = ex.Message
                };

                await logRepository.AddAsync(log);

                _logger.LogError(ex, "Erro ao executar job {JobName} após {ExecutionTime}ms",
                    job.Name, executionTime.TotalMilliseconds);
            }
        }
    }

    public static class CommandExecutor
    {
        public static async Task<string> RunCommandAsync(string command)
        {
            using var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return string.IsNullOrEmpty(error) ? output : $"Erro: {error}\nSaída: {output}";
        }
    }
}
