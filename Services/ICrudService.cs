using Saigor.Repositories.Base;

namespace Saigor.Services
{
    /// <summary>
    /// Interface genérica para operações CRUD.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    public interface ICrudService<T> where T : class
    {
        /// <summary>
        /// Carrega todas as entidades.
        /// </summary>
        Task<List<T>> GetAllAsync();
        
        /// <summary>
        /// Carrega uma entidade por ID.
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        
        /// <summary>
        /// Cria uma nova entidade.
        /// </summary>
        Task<bool> CreateAsync(T entity);
        
        /// <summary>
        /// Atualiza uma entidade existente.
        /// </summary>
        Task<bool> UpdateAsync(T entity);
        
        /// <summary>
        /// Remove uma entidade.
        /// </summary>
        Task<bool> DeleteAsync(T entity);
        
        /// <summary>
        /// Remove uma entidade por ID.
        /// </summary>
        Task<bool> DeleteByIdAsync(int id);
        
        /// <summary>
        /// Filtra entidades com base em um predicado.
        /// </summary>
        Task<List<T>> FilterAsync(Func<T, bool> predicate);
    }
} 