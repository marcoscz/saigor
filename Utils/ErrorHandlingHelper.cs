namespace Saigor.Utils
{
    /// <summary>
    /// Helper para padronizar o tratamento de erros em toda a aplicação
    /// </summary>
    public static class ErrorHandlingHelper
    {
        /// <summary>
        /// Executa uma operação com tratamento de erro padronizado
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="defaultValue">Valor padrão em caso de erro</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>Resultado da operação ou valor padrão</returns>
        public static async Task<T> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<T>> operation,
            ILogger logger,
            T defaultValue,
            string errorMessage,
            string? operationName = null)
        {
            try
            {
                var result = await operation();
                logger.LogDebug("Operação {OperationName} executada com sucesso", operationName ?? "desconhecida");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ErrorMessage} - Operação: {OperationName}", errorMessage, operationName ?? "desconhecida");
                return defaultValue;
            }
        }

        /// <summary>
        /// Executa uma operação síncrona com tratamento de erro padronizado
        /// </summary>
        /// <typeparam name="T">Tipo de retorno</typeparam>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="defaultValue">Valor padrão em caso de erro</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>Resultado da operação ou valor padrão</returns>
        public static T ExecuteWithErrorHandling<T>(
            Func<T> operation,
            ILogger logger,
            T defaultValue,
            string errorMessage,
            string? operationName = null)
        {
            try
            {
                var result = operation();
                logger.LogDebug("Operação {OperationName} executada com sucesso", operationName ?? "desconhecida");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ErrorMessage} - Operação: {OperationName}", errorMessage, operationName ?? "desconhecida");
                return defaultValue;
            }
        }

        /// <summary>
        /// Executa uma operação que retorna bool com tratamento de erro padronizado
        /// </summary>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>True se a operação foi bem-sucedida, false caso contrário</returns>
        public static async Task<bool> ExecuteWithErrorHandlingAsync(
            Func<Task<bool>> operation,
            ILogger logger,
            string errorMessage,
            string? operationName = null)
        {
            try
            {
                var result = await operation();
                if (result)
                {
                    logger.LogDebug("Operação {OperationName} executada com sucesso", operationName ?? "desconhecida");
                }
                else
                {
                    logger.LogWarning("Operação {OperationName} falhou", operationName ?? "desconhecida");
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ErrorMessage} - Operação: {OperationName}", errorMessage, operationName ?? "desconhecida");
                return false;
            }
        }

        /// <summary>
        /// Executa uma operação void com tratamento de erro padronizado
        /// </summary>
        /// <param name="operation">Operação a ser executada</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <param name="operationName">Nome da operação para logging</param>
        /// <returns>True se a operação foi bem-sucedida, false caso contrário</returns>
        public static async Task<bool> ExecuteWithErrorHandlingAsync(
            Func<Task> operation,
            ILogger logger,
            string errorMessage,
            string? operationName = null)
        {
            try
            {
                await operation();
                logger.LogDebug("Operação {OperationName} executada com sucesso", operationName ?? "desconhecida");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ErrorMessage} - Operação: {OperationName}", errorMessage, operationName ?? "desconhecida");
                return false;
            }
        }

        /// <summary>
        /// Valida se uma string não está vazia e registra warning se estiver
        /// </summary>
        /// <param name="value">Valor a ser validado</param>
        /// <param name="parameterName">Nome do parâmetro para logging</param>
        /// <param name="logger">Logger para registrar warnings</param>
        /// <returns>True se a string não está vazia, false caso contrário</returns>
        public static bool ValidateStringParameter(string? value, string parameterName, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                logger.LogWarning("Parâmetro {ParameterName} não pode ser vazio", parameterName);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Valida se um objeto não é nulo e registra warning se for
        /// </summary>
        /// <param name="value">Valor a ser validado</param>
        /// <param name="parameterName">Nome do parâmetro para logging</param>
        /// <param name="logger">Logger para registrar warnings</param>
        /// <returns>True se o objeto não é nulo, false caso contrário</returns>
        public static bool ValidateNotNullParameter(object? value, string parameterName, ILogger logger)
        {
            if (value == null)
            {
                logger.LogWarning("Parâmetro {ParameterName} não pode ser nulo", parameterName);
                return false;
            }
            return true;
        }
    }
} 