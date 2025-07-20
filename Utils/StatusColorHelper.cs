using MudBlazor;
using Saigor.Models;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para centralizar a lógica de cores de status.
    /// </summary>
    public static class StatusColorHelper
    {
        /// <summary>
        /// Retorna a cor apropriada para um status de job.
        /// </summary>
        /// <param name="status">Status do job</param>
        /// <returns>Cor do MudBlazor</returns>
        public static Color GetJobStatusColor(JobStatus status) => status switch
        {
            JobStatus.Rodando => Color.Info,
            JobStatus.Completado => Color.Success,
            JobStatus.Falhou => Color.Error,
            JobStatus.Pendente => Color.Warning,
            _ => Color.Default
        };

        /// <summary>
        /// Retorna a cor apropriada para um status de log.
        /// </summary>
        /// <param name="status">Status do log (string)</param>
        /// <returns>Cor do MudBlazor</returns>
        public static Color GetLogStatusColor(string? status) => status switch
        {
            "Completado" => Color.Success,
            "Falhou" => Color.Error,
            "Rodando" => Color.Info,
            _ => Color.Default
        };

        /// <summary>
        /// Retorna a cor apropriada para um status genérico.
        /// </summary>
        /// <param name="status">Status como string</param>
        /// <returns>Cor do MudBlazor</returns>
        public static Color GetGenericStatusColor(string? status) => status?.ToLowerInvariant() switch
        {
            "completado" or "success" or "sucesso" => Color.Success,
            "falhou" or "error" or "erro" => Color.Error,
            "rodando" or "running" or "info" => Color.Info,
            "pendente" or "pending" or "warning" => Color.Warning,
            _ => Color.Default
        };
    }
} 