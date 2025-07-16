using System.Linq.Expressions;

namespace Saigor.Repositories.Base
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Busca uma entidade pelo id.
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        /// <summary>
        /// Retorna todas as entidades do tipo T.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        /// <summary>
        /// Busca entidades que satisfaçam o predicado informado.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Adiciona uma nova entidade do tipo T.
        /// </summary>
        Task AddAsync(T entity);
        /// <summary>
        /// Atualiza uma entidade existente.
        /// </summary>
        void Update(T entity);
        /// <summary>
        /// Remove uma entidade do tipo T.
        /// </summary>
        void Remove(T entity);
        /// <summary>
        /// Salva as alterações no contexto.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
} 