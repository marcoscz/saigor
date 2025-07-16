using Saigor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saigor.Repositories
{
    public interface IConexaoRepository : Base.IRepository<ConexaoModel>
    {
        // Nenhum método extra necessário, todos já herdados de IRepository<ConexaoModel>
    }
} 