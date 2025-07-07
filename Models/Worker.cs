using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um worker/processo executor de jobs.
    /// </summary>
    public class Worker
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode ter mais que 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Status não pode ter mais que 50 caracteres")]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        public DateTime? LastExecution { get; set; }

        /// <summary>
        /// Jobs associados a este worker.
        /// </summary>
        public virtual ICollection<Job> Jobs { get; set; } = [];
    }
}
