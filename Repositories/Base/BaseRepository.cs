using Microsoft.EntityFrameworkCore;
using Saigor.Utils;
using System.Linq.Expressions;

namespace Saigor.Repositories.Base
{
    /// <summary>
    /// Classe base para repositórios que implementa padrões comuns
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        protected BaseRepository(DbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Busca uma entidade pelo id com tratamento de erro padronizado
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () => await _dbSet.FindAsync(id),
                _logger,
                default(T),
                $"Erro ao buscar {typeof(T).Name} com ID {id}",
                $"Buscar {typeof(T).Name} por ID"
            );
        }

        /// <summary>
        /// Retorna todas as entidades do tipo T com tratamento de erro padronizado
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () => await _dbSet.ToListAsync(),
                _logger,
                new List<T>(),
                $"Erro ao buscar todas as entidades {typeof(T).Name}",
                $"Buscar todas as entidades {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Busca entidades que satisfaçam o predicado informado com tratamento de erro padronizado
        /// </summary>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () => await _dbSet.Where(predicate).ToListAsync(),
                _logger,
                new List<T>(),
                $"Erro ao buscar entidades {typeof(T).Name} com predicado",
                $"Buscar entidades {typeof(T).Name} com filtro"
            );
        }

        /// <summary>
        /// Adiciona uma nova entidade do tipo T com tratamento de erro padronizado
        /// </summary>
        public virtual async Task AddAsync(T entity)
        {
            await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    await _dbSet.AddAsync(entity);
                    return true;
                },
                _logger,
                $"Erro ao adicionar entidade {typeof(T).Name}",
                $"Adicionar entidade {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Atualiza uma entidade existente com tratamento de erro padronizado
        /// </summary>
        public virtual void Update(T entity)
        {
            ErrorHandlingHelper.ExecuteWithErrorHandling(
                () =>
                {
                    _dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                    return true;
                },
                _logger,
                false,
                $"Erro ao atualizar entidade {typeof(T).Name}",
                $"Atualizar entidade {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Remove uma entidade do tipo T com tratamento de erro padronizado
        /// </summary>
        public virtual void Remove(T entity)
        {
            ErrorHandlingHelper.ExecuteWithErrorHandling(
                () =>
                {
                    _dbSet.Remove(entity);
                    return true;
                },
                _logger,
                false,
                $"Erro ao remover entidade {typeof(T).Name}",
                $"Remover entidade {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Salva as alterações no contexto com tratamento de erro padronizado
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () => await _context.SaveChangesAsync(),
                _logger,
                0,
                $"Erro ao salvar alterações para {typeof(T).Name}",
                $"Salvar alterações {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Busca entidades com paginação
        /// </summary>
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    var query = _dbSet.AsQueryable();

                    // Aplicar filtro se fornecido
                    if (filter != null)
                    {
                        query = query.Where(filter);
                    }

                    // Contar total de registros
                    var totalCount = await query.CountAsync();

                    // Aplicar ordenação se fornecida
                    if (orderBy != null)
                    {
                        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                    }

                    // Aplicar paginação
                    var items = await query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return (items, totalCount);
                },
                _logger,
                (new List<T>(), 0),
                $"Erro ao buscar entidades {typeof(T).Name} com paginação",
                $"Buscar entidades {typeof(T).Name} paginadas"
            );
        }

        /// <summary>
        /// Verifica se uma entidade existe
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () => await _dbSet.AnyAsync(predicate),
                _logger,
                false,
                $"Erro ao verificar existência de entidade {typeof(T).Name}",
                $"Verificar existência de entidade {typeof(T).Name}"
            );
        }

        /// <summary>
        /// Conta entidades que satisfaçam o predicado
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    if (predicate != null)
                        return await _dbSet.CountAsync(predicate);
                    return await _dbSet.CountAsync();
                },
                _logger,
                0,
                $"Erro ao contar entidades {typeof(T).Name}",
                $"Contar entidades {typeof(T).Name}"
            );
        }
    }
} 