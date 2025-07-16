using Saigor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saigor.Repositories
{
    public interface ITarefaRepository : Base.IRepository<TarefaModel>
    {
        // Nenhum método extra necessário, todos já herdados de IRepository<TarefaModel>
    }
} 