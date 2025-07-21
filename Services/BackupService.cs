using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using Saigor.Configuration;
using Saigor.Data;

namespace Saigor.Services;

/// <summary>
/// Serviço para backup do sistema
/// </summary>
public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BackupService> _logger;
    private readonly AppSettings _appSettings;
    private readonly string _backupDirectory;

    public BackupService(
        ApplicationDbContext context,
        ILogger<BackupService> logger,
        IOptions<AppSettings> appSettings)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        _context = context;
        _logger = logger;
        _appSettings = appSettings.Value;
        
        _backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        Directory.CreateDirectory(_backupDirectory);
    }

    /// <summary>
    /// Cria um backup completo do sistema
    /// </summary>
    public async Task<BackupResult> CreateBackupAsync()
    {
        var backupId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupName = $"saigor_backup_{timestamp}_{backupId}";
        var backupPath = Path.Combine(_backupDirectory, backupName);

        try
        {
            _logger.LogInformation("Iniciando backup do sistema: {BackupName}", backupName);

            Directory.CreateDirectory(backupPath);

            // Backup do banco de dados
            var dbBackupResult = await CreateDatabaseBackupAsync(backupPath);
            if (!dbBackupResult.Success)
            {
                return BackupResult.Failure($"Falha no backup do banco: {dbBackupResult.Error}");
            }

            // Backup dos logs
            var logsBackupResult = await CreateLogsBackupAsync(backupPath);
            if (!logsBackupResult.Success)
            {
                _logger.LogWarning("Falha no backup dos logs: {Error}", logsBackupResult.Error);
            }

            // Backup das configurações
            var configBackupResult = await CreateConfigBackupAsync(backupPath);
            if (!configBackupResult.Success)
            {
                _logger.LogWarning("Falha no backup das configurações: {Error}", configBackupResult.Error);
            }

            // Criar arquivo ZIP do backup
            var zipPath = $"{backupPath}.zip";
            await CreateBackupZipAsync(backupPath, zipPath);

            // Limpar diretório temporário
            Directory.Delete(backupPath, true);

            var result = new BackupResult
            {
                BackupPath = zipPath,
                BackupSize = new FileInfo(zipPath).Length,
                CreatedAt = DateTime.UtcNow,
                DatabaseBackup = dbBackupResult,
                LogsBackup = logsBackupResult,
                ConfigBackup = configBackupResult
            };

            _logger.LogInformation("Backup criado com sucesso: {BackupPath}, Tamanho: {Size} bytes", 
                zipPath, result.BackupSize);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar backup {BackupName}", backupName);
            return BackupResult.Failure($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Cria backup do banco de dados
    /// </summary>
    private Task<BackupComponentResult> CreateDatabaseBackupAsync(string backupPath)
    {
        try
        {
            var dbPath = Path.Combine(backupPath, "database");
            Directory.CreateDirectory(dbPath);

            // Para SQLite, copiamos o arquivo do banco
            var dbFile = "Saigor.db";
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFile);
            
            if (File.Exists(sourcePath))
            {
                var destPath = Path.Combine(dbPath, dbFile);
                File.Copy(sourcePath, destPath, true);
                
                _logger.LogDebug("Backup do banco de dados criado: {Path}", destPath);
                return Task.FromResult(BackupComponentResult.CreateSuccess(destPath));
            }
            else
            {
                return Task.FromResult(BackupComponentResult.Failure("Arquivo do banco de dados não encontrado"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar backup do banco de dados");
            return Task.FromResult(BackupComponentResult.Failure($"Erro: {ex.Message}"));
        }
    }

    /// <summary>
    /// Cria backup dos logs
    /// </summary>
    private Task<BackupComponentResult> CreateLogsBackupAsync(string backupPath)
    {
        try
        {
            var logsPath = Path.Combine(backupPath, "logs");
            Directory.CreateDirectory(logsPath);

            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (Directory.Exists(logsDirectory))
            {
                var logFiles = Directory.GetFiles(logsDirectory, "*.log");
                foreach (var logFile in logFiles)
                {
                    var fileName = Path.GetFileName(logFile);
                    var destPath = Path.Combine(logsPath, fileName);
                    File.Copy(logFile, destPath, true);
                }

                _logger.LogDebug("Backup dos logs criado: {Count} arquivos", logFiles.Length);
                return Task.FromResult(BackupComponentResult.CreateSuccess(logsPath));
            }
            else
            {
                return Task.FromResult(BackupComponentResult.Failure("Diretório de logs não encontrado"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar backup dos logs");
            return Task.FromResult(BackupComponentResult.Failure($"Erro: {ex.Message}"));
        }
    }

    /// <summary>
    /// Cria backup das configurações
    /// </summary>
    private Task<BackupComponentResult> CreateConfigBackupAsync(string backupPath)
    {
        try
        {
            var configPath = Path.Combine(backupPath, "config");
            Directory.CreateDirectory(configPath);

            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Development.json",
                "appsettings.Production.json"
            };

            var copiedFiles = new List<string>();
            foreach (var configFile in configFiles)
            {
                var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
                if (File.Exists(sourcePath))
                {
                    var destPath = Path.Combine(configPath, configFile);
                    File.Copy(sourcePath, destPath, true);
                    copiedFiles.Add(configFile);
                }
            }

            _logger.LogDebug("Backup das configurações criado: {Count} arquivos", copiedFiles.Count);
            return Task.FromResult(BackupComponentResult.CreateSuccess(configPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar backup das configurações");
            return Task.FromResult(BackupComponentResult.Failure($"Erro: {ex.Message}"));
        }
    }

    /// <summary>
    /// Cria arquivo ZIP do backup
    /// </summary>
    private Task CreateBackupZipAsync(string sourcePath, string zipPath)
    {
        try
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(sourcePath, zipPath);
            _logger.LogDebug("Arquivo ZIP do backup criado: {ZipPath}", zipPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar arquivo ZIP do backup");
            throw;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Lista backups disponíveis
    /// </summary>
    public Task<IEnumerable<BackupInfo>> ListBackupsAsync()
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory, "*.zip")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Select(f => new BackupInfo
                {
                    FileName = Path.GetFileName(f),
                    FilePath = f,
                    FileSize = new FileInfo(f).Length,
                    CreatedAt = File.GetCreationTime(f),
                    LastModified = File.GetLastWriteTime(f)
                })
                .ToList();

            _logger.LogDebug("Backups encontrados: {Count}", backupFiles.Count);
            return Task.FromResult<IEnumerable<BackupInfo>>(backupFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar backups");
            return Task.FromResult<IEnumerable<BackupInfo>>(new List<BackupInfo>());
        }
    }

    /// <summary>
    /// Remove backup antigo
    /// </summary>
    public Task<bool> DeleteBackupAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_backupDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Backup removido: {FileName}", fileName);
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogWarning("Backup não encontrado: {FileName}", fileName);
                return Task.FromResult(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover backup {FileName}", fileName);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Limpa backups antigos
    /// </summary>
    public Task<int> CleanOldBackupsAsync(int daysToKeep = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var deletedCount = 0;

            var backupFiles = Directory.GetFiles(_backupDirectory, "*.zip");
            foreach (var backupFile in backupFiles)
            {
                var fileInfo = new FileInfo(backupFile);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(backupFile);
                    deletedCount++;
                    _logger.LogDebug("Backup antigo removido: {FileName}", fileInfo.Name);
                }
            }

            _logger.LogInformation("Backups antigos removidos: {Count} arquivos", deletedCount);
            return Task.FromResult(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar backups antigos");
            return Task.FromResult(0);
        }
    }
}

/// <summary>
/// Interface para serviço de backup
/// </summary>
public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync();
    Task<IEnumerable<BackupInfo>> ListBackupsAsync();
    Task<bool> DeleteBackupAsync(string fileName);
    Task<int> CleanOldBackupsAsync(int daysToKeep = 30);
}

/// <summary>
/// Resultado de um backup
/// </summary>
public class BackupResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? BackupPath { get; set; }
    public long BackupSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public BackupComponentResult DatabaseBackup { get; set; } = new();
    public BackupComponentResult LogsBackup { get; set; } = new();
    public BackupComponentResult ConfigBackup { get; set; } = new();

    public static BackupResult Failure(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// Resultado de um componente do backup
/// </summary>
public class BackupComponentResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Path { get; set; }

    public static BackupComponentResult CreateSuccess(string path) => new() { Success = true, Path = path };
    public static BackupComponentResult Failure(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// Informações de um backup
/// </summary>
public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
} 