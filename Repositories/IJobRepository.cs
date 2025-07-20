using Saigor.Models;
using Saigor.Repositories.Base;

namespace Saigor.Repositories
{
    public interface IJobRepository : IRepository<JobModel>
    {
        /// <summary>
        /// Atualiza um job existente no banco de dados.
        /// </summary>
        Task UpdateAsync(JobModel job);
        /// <summary>
        /// Busca um job pelo nome.
        /// </summary>
        Task<JobModel?> GetByNameAsync(string name);
        /// <summary>
        /// Verifica se existe um job com o nome informado.
        /// </summary>
        Task<bool> ExistsByNameAsync(string name);
    }
} 