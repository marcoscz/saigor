using Saigor.Utils;

namespace Saigor.Services.Base
{
    /// <summary>
    /// Classe base para serviços que implementa padrões comuns
    /// </summary>
    public abstract class BaseService
    {
        protected readonly ILogger _logger;

        protected BaseService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executa uma operação com tratamento de erro padronizado
        /// </summary>
        protected async Task<T> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<T>> operation,
            T defaultValue,
            string errorMessage,
            string? operationName = null)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                operation,
                _logger,
                defaultValue,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Executa uma operação que retorna bool com tratamento de erro padronizado
        /// </summary>
        protected async Task<bool> ExecuteWithErrorHandlingAsync(
            Func<Task<bool>> operation,
            string errorMessage,
            string? operationName = null)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                operation,
                _logger,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Executa uma operação void com tratamento de erro padronizado
        /// </summary>
        protected async Task<bool> ExecuteWithErrorHandlingAsync(
            Func<Task> operation,
            string errorMessage,
            string? operationName = null)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
                operation,
                _logger,
                errorMessage,
                operationName
            );
        }

        /// <summary>
        /// Valida se uma string não está vazia
        /// </summary>
        protected bool ValidateStringParameter(string? value, string parameterName)
        {
            return ErrorHandlingHelper.ValidateStringParameter(value, parameterName, _logger);
        }

        /// <summary>
        /// Valida se um objeto não é nulo
        /// </summary>
        protected bool ValidateNotNullParameter(object? value, string parameterName)
        {
            return ErrorHandlingHelper.ValidateNotNullParameter(value, parameterName, _logger);
        }

        /// <summary>
        /// Loga informações de forma padronizada
        /// </summary>
        protected void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        /// <summary>
        /// Loga warnings de forma padronizada
        /// </summary>
        protected void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        /// <summary>
        /// Loga erros de forma padronizada
        /// </summary>
        protected void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        /// <summary>
        /// Loga debug de forma padronizada
        /// </summary>
        protected void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }
    }
} 