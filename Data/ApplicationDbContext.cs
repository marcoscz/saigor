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
    /// Processos/Workers do sistema.
    /// </summary>
    public DbSet<Worker> Workers => Set<Worker>();

    /// <summary>
    /// Jobs/Tarefas agendadas.
    /// </summary>
    public DbSet<Job> Jobs => Set<Job>();

    /// <summary>
    /// Logs de execução.
    /// </summary>
    public DbSet<Log> Logs => Set<Log>();

    /// <summary>
    /// Serviços associados aos jobs.
    /// </summary>
    public DbSet<Service> Services => Set<Service>();
    #endregion

    /// <summary>
    /// Configura o modelo de dados e seus relacionamentos.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

        base.OnModelCreating(modelBuilder);

        // Configuração do Worker
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configuração do Job
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Command).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Schedule).IsRequired().HasMaxLength(100);

            // Relacionamento Worker 1:N Job
            entity.HasOne(j => j.Worker)
                .WithMany(w => w.Jobs)
                .HasForeignKey(j => j.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // Configuração do Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);

            // Relacionamento Job 1:N Service
            entity.HasOne(s => s.Job)
                .WithMany(j => j.Services)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
