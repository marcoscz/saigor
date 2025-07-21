using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Saigor.Data;
using Saigor.Models;

namespace Saigor.Services;

/// <summary>
/// Serviço para verificação de saúde do sistema
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<HealthCheckService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(logger);
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica a saúde geral do sistema
    /// </summary>
    public async Task<HealthStatus> CheckHealthAsync()
    {
        var healthStatus = new HealthStatus
        {
            Timestamp = DateTime.UtcNow,
            OverallStatus = HealthState.Healthy
        };

        try
        {
            // Verificar banco de dados
            var dbHealth = await CheckDatabaseHealthAsync();
            healthStatus.Database = dbHealth;

            if (dbHealth.Status == HealthState.Unhealthy)
            {
                healthStatus.OverallStatus = HealthState.Unhealthy;
            }

            // Verificar cache
            var cacheHealth = await CheckCacheHealthAsync();
            healthStatus.Cache = cacheHealth;

            if (cacheHealth.Status == HealthState.Unhealthy && healthStatus.OverallStatus != HealthState.Unhealthy)
            {
                healthStatus.OverallStatus = HealthState.Degraded;
            }

            // Verificar jobs
            var jobsHealth = await CheckJobsHealthAsync();
            healthStatus.Jobs = jobsHealth;

            if (jobsHealth.Status == HealthState.Unhealthy && healthStatus.OverallStatus != HealthState.Unhealthy)
            {
                healthStatus.OverallStatus = HealthState.Degraded;
            }

            // Verificar logs
            var logsHealth = await CheckLogsHealthAsync();
            healthStatus.Logs = logsHealth;

            _logger.LogInformation("Health check concluído. Status geral: {Status}", healthStatus.OverallStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante health check");
            healthStatus.OverallStatus = HealthState.Unhealthy;
            healthStatus.Error = ex.Message;
        }

        return healthStatus;
    }

    /// <summary>
    /// Verifica a saúde do banco de dados
    /// </summary>
    private async Task<ComponentHealth> CheckDatabaseHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            await _context.Database.CanConnectAsync();
            var responseTime = DateTime.UtcNow - startTime;

            return new ComponentHealth
            {
                Status = HealthState.Healthy,
                ResponseTime = responseTime,
                Message = "Database conectado com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar com banco de dados");
            return new ComponentHealth
            {
                Status = HealthState.Unhealthy,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verifica a saúde do cache
    /// </summary>
    private Task<ComponentHealth> CheckCacheHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var testKey = "health_check_test";
            var testValue = "test_value";

            _cacheService.Set(testKey, testValue, TimeSpan.FromSeconds(30));
            var retrievedValue = _cacheService.Get<string>(testKey);

            if (retrievedValue != testValue)
            {
                return Task.FromResult(new ComponentHealth
                {
                    Status = HealthState.Unhealthy,
                    Message = "Cache não está funcionando corretamente"
                });
            }

            var responseTime = DateTime.UtcNow - startTime;
            var stats = _cacheService.GetStatistics();

            return Task.FromResult(new ComponentHealth
            {
                Status = HealthState.Healthy,
                ResponseTime = responseTime,
                Message = $"Cache funcionando. Hit rate: {stats.HitRate:P2}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar cache");
            return Task.FromResult(new ComponentHealth
            {
                Status = HealthState.Unhealthy,
                Message = $"Erro no cache: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Verifica a saúde dos jobs
    /// </summary>
    private async Task<ComponentHealth> CheckJobsHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            var totalJobs = await _context.Jobs.CountAsync();
            var failedJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Falhou);
            var runningJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Rodando);

            var responseTime = DateTime.UtcNow - startTime;
            var failureRate = totalJobs > 0 ? (double)failedJobs / totalJobs : 0;

            var status = failureRate > 0.5 ? HealthState.Unhealthy : 
                        failureRate > 0.2 ? HealthState.Degraded : HealthState.Healthy;

            return new ComponentHealth
            {
                Status = status,
                ResponseTime = responseTime,
                Message = $"Jobs: {totalJobs} total, {runningJobs} rodando, {failedJobs} falharam ({(failureRate * 100):F1}%)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar jobs");
            return new ComponentHealth
            {
                Status = HealthState.Unhealthy,
                Message = $"Erro ao verificar jobs: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verifica a saúde dos logs
    /// </summary>
    private Task<ComponentHealth> CheckLogsHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            var totalLogs = _context.Logs.Count();
            var recentLogs = _context.Logs
                .Where(l => l.ExecutionTime >= DateTime.UtcNow.AddHours(-1))
                .Count();

            var responseTime = DateTime.UtcNow - startTime;

            return Task.FromResult(new ComponentHealth
            {
                Status = HealthState.Healthy,
                ResponseTime = responseTime,
                Message = $"Logs: {totalLogs} total, {recentLogs} na última hora"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar logs");
            return Task.FromResult(new ComponentHealth
            {
                Status = HealthState.Unhealthy,
                Message = $"Erro ao verificar logs: {ex.Message}"
            });
        }
    }
}

/// <summary>
/// Interface para serviço de health check
/// </summary>
public interface IHealthCheckService
{
    Task<HealthStatus> CheckHealthAsync();
}

/// <summary>
/// Status de saúde do sistema
/// </summary>
public class HealthStatus
{
    public DateTime Timestamp { get; set; }
    public HealthState OverallStatus { get; set; }
    public ComponentHealth Database { get; set; } = new();
    public ComponentHealth Cache { get; set; } = new();
    public ComponentHealth Jobs { get; set; } = new();
    public ComponentHealth Logs { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Saúde de um componente específico
/// </summary>
public class ComponentHealth
{
    public HealthState Status { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Estados de saúde possíveis
/// </summary>
public enum HealthState
{
    Healthy,
    Degraded,
    Unhealthy
} 