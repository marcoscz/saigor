using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa uma tarefa do sistema.
    /// </summary>
    public class TarefaModel
    {
        /// <summary>
        /// Identificador único da tarefa.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome da tarefa.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Função associada à tarefa.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Funcao { get; set; } = string.Empty;

        /// <summary>
        /// Parâmetros opcionais da tarefa.
        /// </summary>
        [StringLength(1000)]
        public string? Parametros { get; set; }

        /// <summary>
        /// Status atual da tarefa.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Indica se a tarefa está ativa.
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Data de criação da tarefa.
        /// </summary>
        public DateTime DataCriacao { get; set; }
    }
} 