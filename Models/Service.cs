using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um serviço associado a um job no sistema.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Identificador único do serviço.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do serviço.
        /// </summary>
        [Required(ErrorMessage = "Nome do serviço é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode ter mais que 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do Job associado.
        /// </summary>
        [Required(ErrorMessage = "Job é obrigatório")]
        [Display(Name = "Job")]
        public int JobId { get; set; }

        /// <summary>
        /// Navegação para o Job relacionado.
        /// </summary>
        public virtual Job Job { get; set; } = null!;

        /// <summary>
        /// Status atual do serviço.
        /// </summary>
        [StringLength(50, ErrorMessage = "Status não pode ter mais que 50 caracteres")]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        /// <summary>
        /// Data/hora da última execução.
        /// </summary>
        [Display(Name = "Última Execução")]
        public DateTime? LastExecution { get; set; }
    }
}