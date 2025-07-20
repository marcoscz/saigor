using Saigor.Models;

namespace Saigor.Domain.Interfaces;

/// <summary>
/// Interface para operações de domínio relacionadas a Jobs
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Inicia um job pelo nome
    /// </summary>
    Task<bool> StartJobAsync(string jobName);
    
    /// <summary>
    /// Para um job pelo nome
    /// </summary>
    Task<bool> StopJobAsync(string jobName);
    
    /// <summary>
    /// Agenda um novo job
    /// </summary>
    Task<bool> ScheduleJobAsync(JobModel job);
    
    /// <summary>
    /// Obtém o status atual de um job
    /// </summary>
    Task<string?> GetJobStatusAsync(string jobName);
    
    /// <summary>
    /// Verifica se um job está agendado
    /// </summary>
    Task<bool> IsJobScheduledAsync(int jobId);
} 