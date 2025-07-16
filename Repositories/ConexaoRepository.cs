using Saigor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Saigor.Repositories
{
    public class ConexaoRepository : IConexaoRepository, Base.IRepository<ConexaoModel>
    {
        private readonly Saigor.Data.ApplicationDbContext _context;
        public ConexaoRepository(Saigor.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConexaoModel?> GetByIdAsync(int id)
        {
            return await _context.Conexoes.FindAsync(id);
        }

        public async Task<IEnumerable<ConexaoModel>> GetAllAsync()
        {
            return await _context.Conexoes.ToListAsync();
        }

        public async Task<IEnumerable<ConexaoModel>> FindAsync(Expression<Func<ConexaoModel, bool>> predicate)
        {
            return await _context.Conexoes.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(ConexaoModel conexao)
        {
            await _context.Conexoes.AddAsync(conexao);
        }

        public void Update(ConexaoModel conexao)
        {
            _context.Conexoes.Update(conexao);
        }

        public void Remove(ConexaoModel conexao)
        {
            _context.Conexoes.Remove(conexao);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 