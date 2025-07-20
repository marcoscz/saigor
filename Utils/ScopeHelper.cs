using Microsoft.Extensions.DependencyInjection;

namespace Saigor.Utils
{
    /// <summary>
    /// Helper para padronizar a criação e uso de scopes de dependência
    /// </summary>
    public static class ScopeHelper
    {
        /// <summary>
        /// Executa uma operação dentro de um scope de dependência
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="scopeFactory">Factory para criar scopes</param>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="defaultValue">Valor padrão em caso de erro</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>Resultado da operação ou valor padrão</returns>
        public static async Task<T> ExecuteInScopeAsync<T>(
            IServiceScopeFactory scopeFactory,
            Func<IServiceProvider, Task<T>> operation,
            ILogger logger,
            T defaultValue,
            string errorMessage,
            string? operationName = null)
        {
            using var scope = scopeFactory.CreateScope();
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                () => operation(scope.ServiceProvider),
                logger,
                defaultValue,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Executa uma operação que retorna bool dentro de um scope de dependência
        /// </summary>
        /// <param name="scopeFactory">Factory para criar scopes</param>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>True se a operação foi bem-sucedida, false caso contrário</returns>
        public static async Task<bool> ExecuteInScopeAsync(
            IServiceScopeFactory scopeFactory,
            Func<IServiceProvider, Task<bool>> operation,
            ILogger logger,
            string errorMessage,
            string? operationName = null)
        {
            using var scope = scopeFactory.CreateScope();
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                () => operation(scope.ServiceProvider),
                logger,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Executa uma operação void dentro de um scope de dependência
        /// </summary>
        /// <param name="scopeFactory">Factory para criar scopes</param>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>True se a operação foi bem-sucedida, false caso contrário</returns>
        public static async Task<bool> ExecuteInScopeAsync(
            IServiceScopeFactory scopeFactory,
            Func<IServiceProvider, Task> operation,
            ILogger logger,
            string errorMessage,
            string? operationName = null)
        {
            using var scope = scopeFactory.CreateScope();
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                () => operation(scope.ServiceProvider),
                logger,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Obtém um serviço do scope de forma segura
        /// </summary>
        /// <typeparam name="T">Tipo do serviço</typeparam>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="serviceName">Nome do serviço para logging</param>
        /// <returns>Instância do serviço ou null se não encontrado</returns>
        public static T? GetServiceSafely<T>(IServiceProvider serviceProvider, ILogger logger, string? serviceName = null)
        {
            return ErrorHandlingHelper.ExecuteWithErrorHandling(
                () => serviceProvider.GetService<T>(),
                logger,
                default(T),
                $"Erro ao obter serviço {serviceName ?? typeof(T).Name}",
                $"Obter {serviceName ?? typeof(T).Name}"
            );
        }

        /// <summary>
        /// Obtém um serviço requerido do scope de forma segura
        /// </summary>
        /// <typeparam name="T">Tipo do serviço</typeparam>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="serviceName">Nome do serviço para logging</param>
        /// <returns>Instância do serviço ou null se não encontrado</returns>
        public static T? GetRequiredServiceSafely<T>(IServiceProvider serviceProvider, ILogger logger, string? serviceName = null) where T : notnull
        {
            return ErrorHandlingHelper.ExecuteWithErrorHandling(
                () => serviceProvider.GetRequiredService<T>(),
                logger,
                default(T),
                $"Erro ao obter serviço requerido {serviceName ?? typeof(T).Name}",
                $"Obter {serviceName ?? typeof(T).Name}"
            );
        }
    }
} 