using Saigor.Repositories.Base;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para padronizar o carregamento de dados.
    /// </summary>
    public static class DataLoaderHelper
    {
        /// <summary>
        /// Método genérico para executar operações de dados com tratamento de erro padronizado.
        /// </summary>
        private static async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, ILogger logger, T defaultValue, string errorMessage)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(operation, logger, defaultValue, errorMessage);
        }

        /// <summary>
        /// Carrega dados de forma padronizada com tratamento de erro.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para carregar os dados</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Lista de entidades ou lista vazia em caso de erro</returns>
        public static Task<List<T>> LoadDataAsync<T>(
            IRepository<T> repository, 
            ILogger logger, 
            string errorMessage = "Erro ao carregar dados") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                async () => (await repository.GetAllAsync()).ToList(),
                logger,
                new List<T>(),
                errorMessage
            );
        }

        /// <summary>
        /// Carrega uma entidade específica por ID.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para carregar os dados</param>
        /// <param name="id">ID da entidade</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Entidade encontrada ou null</returns>
        public static Task<T?> LoadEntityByIdAsync<T>(
            IRepository<T> repository, 
            int id, 
            ILogger logger, 
            string errorMessage = "Erro ao carregar entidade") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                () => repository.GetByIdAsync(id),
                logger,
                null,
                errorMessage
            );
        }

        /// <summary>
        /// Carrega dados com filtro personalizado.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para carregar os dados</param>
        /// <param name="predicate">Predicado para filtrar os dados</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Lista filtrada de entidades</returns>
        public static Task<List<T>> LoadDataWithFilterAsync<T>(
            IRepository<T> repository, 
            Func<T, bool> predicate, 
            ILogger logger, 
            string errorMessage = "Erro ao carregar dados filtrados") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                async () => (await repository.GetAllAsync()).Where(predicate).ToList(),
                logger,
                new List<T>(),
                errorMessage
            );
        }

        /// <summary>
        /// Salva dados de forma padronizada com tratamento de erro.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para salvar os dados</param>
        /// <param name="entity">Entidade a ser salva</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>True se salvou com sucesso, false caso contrário</returns>
        public static Task<bool> SaveDataAsync<T>(
            IRepository<T> repository, 
            T entity, 
            ILogger logger, 
            string errorMessage = "Erro ao salvar dados") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                async () => {
                    await repository.AddAsync(entity);
                    await repository.SaveChangesAsync();
                    return true;
                },
                logger,
                false,
                errorMessage
            );
        }

        /// <summary>
        /// Atualiza dados de forma padronizada com tratamento de erro.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para atualizar os dados</param>
        /// <param name="entity">Entidade a ser atualizada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>True se atualizou com sucesso, false caso contrário</returns>
        public static Task<bool> UpdateDataAsync<T>(
            IRepository<T> repository, 
            T entity, 
            ILogger logger, 
            string errorMessage = "Erro ao atualizar dados") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                async () => {
                    repository.Update(entity);
                    await repository.SaveChangesAsync();
                    return true;
                },
                logger,
                false,
                errorMessage
            );
        }

        /// <summary>
        /// Remove dados de forma padronizada com tratamento de erro.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="repository">Repositório para remover os dados</param>
        /// <param name="entity">Entidade a ser removida</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>True se removeu com sucesso, false caso contrário</returns>
        public static Task<bool> DeleteDataAsync<T>(
            IRepository<T> repository, 
            T entity, 
            ILogger logger, 
            string errorMessage = "Erro ao remover dados") where T : class
        {
            return ExecuteWithErrorHandlingAsync(
                async () => {
                    repository.Remove(entity);
                    await repository.SaveChangesAsync();
                    return true;
                },
                logger,
                false,
                errorMessage
            );
        }
    }
} 