using Saigor.Data;
using Saigor.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saigor.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna o total de jobs cadastrados.
        /// </summary>
        public async Task<int> GetTotalJobsAsync() => await _context.Jobs.CountAsync();
        /// <summary>
        /// Retorna o total de tarefas cadastradas.
        /// </summary>
        public async Task<int> GetTotalTarefasAsync() => await _context.Tarefas.CountAsync();
        /// <summary>
        /// Retorna o total de logs cadastrados.
        /// </summary>
        public async Task<int> GetTotalLogsAsync() => await _context.Logs.CountAsync();

        /// <summary>
        /// Retorna estatísticas gerais do dashboard.
        /// </summary>
        public async Task<DashboardStatistics> GetStatisticsAsync()
        {
            var jobs = await _context.Jobs.ToListAsync();
            var activeJobs = jobs.Count(j => j.Status == JobStatus.Rodando || j.Status == JobStatus.Completado);
            return new DashboardStatistics
            {
                TotalJobs = jobs.Count,
                TotalTarefas = await GetTotalTarefasAsync(),
                TotalLogs = await GetTotalLogsAsync(),
                ActiveJobs = activeJobs,
                JobsByStatus = await GetJobsCountByStatusAsync(),
                LogsByStatus = await GetLogsByStatusAsync()
            };
        }

        /// <summary>
        /// Retorna os 5 jobs mais recentes.
        /// </summary>
        public async Task<IEnumerable<JobModel>> GetRecentJobsAsync()
        {
            return await _context.Jobs
                .OrderByDescending(j => j.Id)
                .Take(5)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna os 5 logs mais recentes.
        /// </summary>
        public async Task<IEnumerable<LogModel>> GetRecentLogsAsync()
        {
            return await _context.Logs
                .OrderByDescending(l => l.Id)
                .Take(5)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna um resumo dos jobs filtrados por status.
        /// </summary>
        public async Task<IEnumerable<JobSummaryDto>> GetJobsByStatusAsync(JobStatus status)
        {
            return await _context.Jobs
                .Where(j => j.Status == status)
                .Select(j => new JobSummaryDto
                {
                    Id = j.Id,
                    Name = j.Name,
                    Command = j.Command,
                    Schedule = j.Schedule,
                    Status = j.Status
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Dictionary<JobStatus, int>> GetJobsCountByStatusAsync()
        {
            return await _context.Jobs
                .AsNoTracking()
                .GroupBy(j => j.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        /// <summary>
        /// Retorna a contagem de logs agrupados por status.
        /// </summary>
        public async Task<Dictionary<string, int>> GetLogsByStatusAsync()
        {
            return await _context.Logs
                .AsNoTracking()
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => (string)(x.Status ?? string.Empty), x => x.Count);
        }
    }
} 