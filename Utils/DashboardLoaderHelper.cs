using Saigor.Services;
using Saigor.Models;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para padronizar o carregamento de dados do dashboard.
    /// </summary>
    public static class DashboardLoaderHelper
    {
        /// <summary>
        /// Método genérico para executar operações do dashboard com tratamento de erro padronizado.
        /// </summary>
        private static async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, ILogger logger, T defaultValue, string errorMessage)
        {
            return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(operation, logger, defaultValue, errorMessage);
        }

        /// <summary>
        /// Carrega estatísticas do dashboard de forma padronizada.
        /// </summary>
        /// <param name="dashboardService">Serviço do dashboard</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Estatísticas do dashboard ou objeto vazio em caso de erro</returns>
        public static Task<DashboardStatistics> LoadStatisticsAsync(
            IDashboardService dashboardService,
            ILogger logger,
            string errorMessage = "Erro ao carregar estatísticas")
        {
            return ExecuteWithErrorHandlingAsync<DashboardStatistics>(
                () => dashboardService.GetStatisticsAsync(),
                logger,
                new DashboardStatistics(),
                errorMessage
            );
        }

        /// <summary>
        /// Carrega jobs recentes de forma padronizada.
        /// </summary>
        /// <param name="dashboardService">Serviço do dashboard</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Lista de jobs recentes ou lista vazia em caso de erro</returns>
        public static Task<List<JobModel>> LoadRecentJobsAsync(
            IDashboardService dashboardService,
            ILogger logger,
            string errorMessage = "Erro ao carregar jobs recentes")
        {
            return ExecuteWithErrorHandlingAsync<List<JobModel>>(
                async () => (await dashboardService.GetRecentJobsAsync()).ToList(),
                logger,
                new List<JobModel>(),
                errorMessage
            );
        }

        /// <summary>
        /// Carrega logs recentes de forma padronizada.
        /// </summary>
        /// <param name="dashboardService">Serviço do dashboard</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Lista de logs recentes ou lista vazia em caso de erro</returns>
        public static Task<List<LogModel>> LoadRecentLogsAsync(
            IDashboardService dashboardService,
            ILogger logger,
            string errorMessage = "Erro ao carregar logs recentes")
        {
            return ExecuteWithErrorHandlingAsync<List<LogModel>>(
                async () => (await dashboardService.GetRecentLogsAsync()).ToList(),
                logger,
                new List<LogModel>(),
                errorMessage
            );
        }

        /// <summary>
        /// Carrega jobs por status de forma padronizada.
        /// </summary>
        /// <param name="dashboardService">Serviço do dashboard</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Dicionário de jobs por status ou dicionário vazio em caso de erro</returns>
        public static Task<Dictionary<JobStatus, int>> LoadJobsByStatusAsync(
            IDashboardService dashboardService,
            ILogger logger,
            string errorMessage = "Erro ao carregar jobs por status")
        {
            return ExecuteWithErrorHandlingAsync<Dictionary<JobStatus, int>>(
                () => dashboardService.GetJobsCountByStatusAsync(),
                logger,
                new Dictionary<JobStatus, int>(),
                errorMessage
            );
        }

        /// <summary>
        /// Carrega logs por status de forma padronizada.
        /// </summary>
        /// <param name="dashboardService">Serviço do dashboard</param>
        /// <param name="logger">Logger para registrar erros</param>
        /// <param name="errorMessage">Mensagem de erro personalizada</param>
        /// <returns>Dicionário de logs por status ou dicionário vazio em caso de erro</returns>
        public static Task<Dictionary<string, int>> LoadLogsByStatusAsync(
            IDashboardService dashboardService,
            ILogger logger,
            string errorMessage = "Erro ao carregar logs por status")
        {
            return ExecuteWithErrorHandlingAsync<Dictionary<string, int>>(
                () => dashboardService.GetLogsByStatusAsync(),
                logger,
                new Dictionary<string, int>(),
                errorMessage
            );
        }
    }
} 