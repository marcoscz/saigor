using System.ComponentModel.DataAnnotations;

namespace Saigor.Models
{
    /// <summary>
    /// Representa uma conexão de sistema.
    /// </summary>
    public class ConexaoModel
    {
        /// <summary>
        /// Identificador único da conexão.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo do conector.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Conector { get; set; } = string.Empty;

        /// <summary>
        /// Endereço do servidor.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Servidor { get; set; } = string.Empty;

        /// <summary>
        /// Ambiente da conexão (ex: produção, homologação).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Ambiente { get; set; } = string.Empty;

        /// <summary>
        /// Nome da conexão.
        /// </summary>
        [StringLength(100)]
        public string? Nome { get; set; }

        /// <summary>
        /// Descrição da conexão.
        /// </summary>
        [StringLength(500)]
        public string? Descricao { get; set; }

        /// <summary>
        /// Indica se a conexão está ativa.
        /// </summary>
        public bool Ativo { get; set; }
    }
} 