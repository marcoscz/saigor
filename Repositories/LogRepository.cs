using Saigor.Data;
using Saigor.Models;
using Saigor.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Saigor.Repositories
{
    public class LogRepository : Repository<LogModel>, ILogRepository
    {
        public LogRepository(ApplicationDbContext context) : base(context)
        {
        }
        // Métodos específicos para logs podem ser implementados aqui

        /// <summary>
        /// Remove um log pelo id.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var log = await _dbSet.FindAsync(id);
            if (log != null)
            {
                _dbSet.Remove(log);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Busca logs em um intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<LogModel>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(l => l.ExecutionTime >= start && l.ExecutionTime <= end)
                .ToListAsync();
        }

        /// <summary>
        /// Busca logs por status.
        /// </summary>
        public async Task<IEnumerable<LogModel>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(l => l.Status == status)
                .ToListAsync();
        }
    }
} 