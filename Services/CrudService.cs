using Saigor.Repositories.Base;
using Saigor.Utils;

namespace Saigor.Services
{
    /// <summary>
    /// Implementação genérica de serviço CRUD.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    public class CrudService<T> : ICrudService<T> where T : class
    {
        private readonly IRepository<T> _repository;
        private readonly ILogger<CrudService<T>> _logger;

        public CrudService(IRepository<T> repository, ILogger<CrudService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await DataLoaderHelper.LoadDataAsync(_repository, _logger, $"Erro ao carregar {typeof(T).Name}");
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await DataLoaderHelper.LoadEntityByIdAsync(_repository, id, _logger, $"Erro ao carregar {typeof(T).Name} com ID {id}");
        }

        public async Task<bool> CreateAsync(T entity)
        {
            return await DataLoaderHelper.SaveDataAsync(_repository, entity, _logger, $"Erro ao criar {typeof(T).Name}");
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            return await DataLoaderHelper.UpdateDataAsync(_repository, entity, _logger, $"Erro ao atualizar {typeof(T).Name}");
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            return await DataLoaderHelper.DeleteDataAsync(_repository, entity, _logger, $"Erro ao remover {typeof(T).Name}");
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning($"{typeof(T).Name} com ID {id} não encontrado para remoção");
                return false;
            }
            return await DeleteAsync(entity);
        }

        public async Task<List<T>> FilterAsync(Func<T, bool> predicate)
        {
            return await DataLoaderHelper.LoadDataWithFilterAsync(_repository, predicate, _logger, $"Erro ao filtrar {typeof(T).Name}");
        }
    }
} 