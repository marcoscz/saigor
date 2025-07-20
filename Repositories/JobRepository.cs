using Saigor.Data;
using Saigor.Models;
using Saigor.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Saigor.Repositories
{
    public class JobRepository : Repository<JobModel>, IJobRepository
    {
        public JobRepository(ApplicationDbContext context) : base(context)
        {
        }
        // Métodos específicos para jobs podem ser implementados aqui

        /// <summary>
        /// Atualiza um job existente no banco de dados.
        /// </summary>
        public async Task UpdateAsync(JobModel job)
        {
            _context.Entry(job).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca um job pelo nome.
        /// </summary>
        public async Task<JobModel?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(j => j.Name == name);
        }

        /// <summary>
        /// Verifica se existe um job com o nome informado.
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbSet.AnyAsync(j => j.Name == name);
        }
    }
} 