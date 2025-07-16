using Saigor.Models;
using Saigor.Repositories.Base;

namespace Saigor.Repositories
{
    public interface ILogRepository : IRepository<Log>
    {
        /// <summary>
        /// Remove um log pelo id.
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Busca logs em um intervalo de datas.
        /// </summary>
        Task<IEnumerable<Log>> GetByDateRangeAsync(DateTime start, DateTime end);
        /// <summary>
        /// Busca logs por status.
        /// </summary>
        Task<IEnumerable<Log>> GetByStatusAsync(string status);
    }
} 