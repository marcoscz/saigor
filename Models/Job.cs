using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa um job agendado no sistema.
    /// </summary>
    public class Job
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode ter mais que 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Comando é obrigatório")]
        [StringLength(500, ErrorMessage = "Comando não pode ter mais que 500 caracteres")]
        [Display(Name = "Comando")]
        public string Command { get; set; } = string.Empty;

        [Required(ErrorMessage = "Agendamento é obrigatório")]
        [StringLength(100, ErrorMessage = "Agendamento não pode ter mais que 100 caracteres")]
        [RegularExpression(@"^(\*|([0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])) (\*|([0-9]|1[0-9]|2[0-3])) (\*|([1-9]|1[0-9]|2[0-9]|3[0-1])) (\*|([1-9]|1[0-2])) (\*|([0-6]))$",
            ErrorMessage = "Formato de Cron inválido")]
        [Display(Name = "Agendamento")]
        public string Schedule { get; set; } = string.Empty;

        [Display(Name = "Última Execução")]
        public DateTime? LastExecution { get; set; }

        [Display(Name = "Status")]
        public JobStatus Status { get; set; } = JobStatus.Rodando;

        [Required(ErrorMessage = "Worker é obrigatório")]
        [Display(Name = "Worker")]
        public int WorkerId { get; set; }

        /// <summary>
        /// Navegação para o Worker relacionado.
        /// </summary>
        public virtual Worker Worker { get; set; } = null!;

        /// <summary>
        /// Logs de execução deste job.
        /// </summary>
        public virtual ICollection<Log> Logs { get; set; } = [];

        /// <summary>
        /// Serviços relacionados a este job.
        /// </summary>
        public virtual ICollection<Service> Services { get; set; } = [];
    }

    /// <summary>
    /// Status possíveis para um job.
    /// </summary>
    public enum JobStatus
    {
        Pendente,
        Rodando,
        Completado,
        Falhou,
        Parado
    }
}