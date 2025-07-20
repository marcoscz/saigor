using Saigor.Models;
using Saigor.Repositories.Base;

namespace Saigor.Repositories
{
    public interface ILogRepository : IRepository<LogModel>
    {
        /// <summary>
        /// Remove um log pelo id.
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Busca logs em um intervalo de datas.
        /// </summary>
        Task<IEnumerable<LogModel>> GetByDateRangeAsync(DateTime start, DateTime end);
        /// <summary>
        /// Busca logs por status.
        /// </summary>
        Task<IEnumerable<LogModel>> GetByStatusAsync(string status);
    }
} 