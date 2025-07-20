namespace Saigor.Services;

/// <summary>
/// Interface para execução de comandos do sistema
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executa um comando do sistema
    /// </summary>
    Task<CommandExecutionResult> ExecuteCommandAsync(string command);

    /// <summary>
    /// Executa um script PowerShell
    /// </summary>
    Task<CommandExecutionResult> ExecutePowerShellAsync(string script);
} 