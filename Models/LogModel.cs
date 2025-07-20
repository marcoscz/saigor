using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um log de execução de um job.
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// Identificador único do log.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Identificador do job relacionado.
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// Data/hora da execução.
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Saída (output) da execução.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Output não pode ter mais que 1000 caracteres")]
        [Display(Name = "Output")]
        public string? Output { get; set; }

        /// <summary>
        /// Status da execução.
        /// </summary>
        [StringLength(50, ErrorMessage = "Status não pode ter mais que 50 caracteres")]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        /// <summary>
        /// Navegação para o Job relacionado.
        /// </summary>
        public virtual JobModel? Job { get; set; }
    }
} 