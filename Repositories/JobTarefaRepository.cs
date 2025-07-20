using Saigor.Data;
using Saigor.Models;
using Saigor.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Saigor.Repositories
{
    public class JobTarefaRepository : Repository<JobTarefaModel>, IJobTarefaRepository
    {
        public JobTarefaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<JobTarefaModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(jt => jt.Job)
                .Include(jt => jt.Tarefa)
                .Include(jt => jt.Conexao)
                .ToListAsync();
        }

        public override async Task<JobTarefaModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(jt => jt.Job)
                .Include(jt => jt.Tarefa)
                .Include(jt => jt.Conexao)
                .FirstOrDefaultAsync(jt => jt.Id == id);
        }
    }
} 