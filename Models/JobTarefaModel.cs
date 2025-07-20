using System.ComponentModel.DataAnnotations.Schema;

namespace Saigor.Models
{
    public class JobTarefaModel
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int TarefaId { get; set; }
        public int ConexoesId { get; set; }
        public int Ordem { get; set; }

        [ForeignKey("JobId")]
        public JobModel Job { get; set; } = default!;
        [ForeignKey("TarefaId")]
        public TarefaModel Tarefa { get; set; } = default!;
        [ForeignKey("ConexoesId")]
        public ConexaoModel Conexao { get; set; } = default!;
    }
} 