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
        [Display(Name = "Agendamento")]
        public string Schedule { get; set; } = string.Empty;

        [Display(Name = "Última Execução")]
        public DateTime? LastExecution { get; set; }

        [Display(Name = "Status")]
        public JobStatus Status { get; set; } = JobStatus.Rodando;

        /// <summary>
        /// Logs de execução deste job.
        /// </summary>
        public virtual ICollection<Log> Logs { get; set; } = [];

        /// <summary>
        /// Associações com tarefas.
        /// </summary>
        public virtual ICollection<JobTarefa> JobTarefas { get; set; } = [];
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