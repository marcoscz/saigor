using Saigor.Data;
using Saigor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Saigor.Repositories
{
    public class TarefaRepository : ITarefaRepository, Base.IRepository<TarefaModel>
    {
        private readonly ApplicationDbContext _context;
        public TarefaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TarefaModel?> GetByIdAsync(int id)
        {
            return await _context.Tarefas.FindAsync(id);
        }

        public async Task<IEnumerable<TarefaModel>> GetAllAsync()
        {
            return await _context.Tarefas.ToListAsync();
        }

        public async Task<IEnumerable<TarefaModel>> FindAsync(Expression<Func<TarefaModel, bool>> predicate)
        {
            return await _context.Tarefas.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(TarefaModel tarefa)
        {
            await _context.Tarefas.AddAsync(tarefa);
        }

        public void Update(TarefaModel tarefa)
        {
            _context.Tarefas.Update(tarefa);
        }

        public void Remove(TarefaModel tarefa)
        {
            _context.Tarefas.Remove(tarefa);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 