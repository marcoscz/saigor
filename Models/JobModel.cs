using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um job agendado no sistema.
    /// </summary>
    public class JobModel
    {
        /// <summary>
        /// Identificador único do job.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do job.
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode ter mais que 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Comando a ser executado pelo job.
        /// </summary>
        [Required(ErrorMessage = "Comando é obrigatório")]
        [StringLength(500, ErrorMessage = "Comando não pode ter mais que 500 caracteres")]
        [Display(Name = "Comando")]
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Expressão CRON de agendamento do job.
        /// </summary>
        [Required(ErrorMessage = "Agendamento é obrigatório")]
        [StringLength(100, ErrorMessage = "Agendamento não pode ter mais que 100 caracteres")]
        [Display(Name = "Agendamento")]
        public string Schedule { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora da última execução do job.
        /// </summary>
        [Display(Name = "Última Execução")]
        public DateTime? LastExecution { get; set; }

        /// <summary>
        /// Status atual do job.
        /// </summary>
        [Display(Name = "Status")]
        public JobStatus Status { get; set; } = JobStatus.Rodando;

        /// <summary>
        /// Indica se o job está ativo.
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Ordem de execução do job.
        /// </summary>
        [Display(Name = "Ordem")]
        public int Ordem { get; set; } = 0;

        /// <summary>
        /// Logs de execução deste job.
        /// </summary>
        public virtual ICollection<LogModel> Logs { get; set; } = [];
    }
} 