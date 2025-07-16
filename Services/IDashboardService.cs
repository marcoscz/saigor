using System.Collections.Generic;
using System.Threading.Tasks;
using Saigor.Models;

namespace Saigor.Services
{
    public interface IDashboardService
    {
        Task<int> GetTotalJobsAsync();
        Task<int> GetTotalTarefasAsync();
        Task<int> GetTotalLogsAsync();
        /// <summary>
        /// Retorna um resumo dos jobs filtrados por status.
        /// </summary>
        Task<IEnumerable<JobSummaryDto>> GetJobsByStatusAsync(JobStatus status);
        /// <summary>
        /// Retorna a contagem de jobs agrupados por status.
        /// </summary>
        Task<Dictionary<JobStatus, int>> GetJobsCountByStatusAsync();
        Task<DashboardStatistics> GetStatisticsAsync();
        Task<IEnumerable<Job>> GetRecentJobsAsync();
        Task<IEnumerable<Log>> GetRecentLogsAsync();
        Task<Dictionary<string, int>> GetLogsByStatusAsync();
    }
} 