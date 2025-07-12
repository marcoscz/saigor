using Microsoft.EntityFrameworkCore;
using Saigor.Models;

namespace Saigor.Data;

/// <summary>
/// Contexto principal do Entity Framework Core para o sistema Saigor.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializa uma nova instância do contexto.
    /// </summary>
    /// <param name="options">Opções do contexto.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    #region DbSets
    /// <summary>
    /// Jobs/Tarefas agendadas.
    /// </summary>
    public DbSet<Job> Jobs => Set<Job>();

    /// <summary>
    /// Logs de execução.
    /// </summary>
    public DbSet<Log> Logs => Set<Log>();

    /// <summary>
    /// Conexões de banco de dados.
    /// </summary>
    public DbSet<ConexaoModel> Conexoes => Set<ConexaoModel>();

    /// <summary>
    /// Tarefas do sistema.
    /// </summary>
    public DbSet<TarefaModel> Tarefas => Set<TarefaModel>();

    /// <summary>
    /// Associações entre Jobs e Tarefas.
    /// </summary>
    public DbSet<JobTarefa> JobTarefas => Set<JobTarefa>();
    #endregion

    /// <summary>
    /// Configura o modelo de dados e seus relacionamentos.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

        base.OnModelCreating(modelBuilder);

        // Configuração do Job
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Command).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Schedule).IsRequired().HasMaxLength(100);
        });

        // Configuração do Log
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExecutionTime);

            entity.Property(e => e.Output).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(50);

            // Relacionamento Job 1:N Log
            entity.HasOne(l => l.Job)
                .WithMany(j => j.Logs)
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da Conexao
        modelBuilder.Entity<ConexaoModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Conector);
            entity.HasIndex(e => e.Ambiente);
            entity.HasIndex(e => e.Ativo);

            entity.Property(e => e.Conector).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Servidor).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Ambiente).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(500);
        });

        // Configuração da Tarefa
        modelBuilder.Entity<TarefaModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nome);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Ativo);

            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Funcao).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Parametros).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        // Configuração da JobTarefa
        modelBuilder.Entity<JobTarefa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.TarefaId);
            entity.HasIndex(e => e.Ativo);

            entity.Property(e => e.Observacoes).HasMaxLength(500);

            // Relacionamento JobTarefa N:1 Job
            entity.HasOne(jt => jt.Job)
                .WithMany(j => j.JobTarefas)
                .HasForeignKey(jt => jt.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento JobTarefa N:1 Tarefa
            entity.HasOne(jt => jt.Tarefa)
                .WithMany(t => t.JobTarefas)
                .HasForeignKey(jt => jt.TarefaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar duplicatas
            entity.HasIndex(e => new { e.JobId, e.TarefaId }).IsUnique();
        });
    }
}
