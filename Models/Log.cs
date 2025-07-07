using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um log de execução de um job.
    /// </summary>
    public class Log
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public DateTime ExecutionTime { get; set; }

        [StringLength(1000, ErrorMessage = "Output não pode ter mais que 1000 caracteres")]
        [Display(Name = "Output")]
        public string? Output { get; set; }

        [StringLength(50, ErrorMessage = "Status não pode ter mais que 50 caracteres")]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        /// <summary>
        /// Navegação para o Job relacionado.
        /// </summary>
        public virtual Job? Job { get; set; }
    }
}