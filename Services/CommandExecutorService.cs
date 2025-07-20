using System.Diagnostics;
using Saigor.Shared;
using Saigor.Utils;

namespace Saigor.Services;

/// <summary>
/// Serviço para execução de comandos do sistema
/// </summary>
public class CommandExecutorService : ICommandExecutor
{
    private readonly ILogger<CommandExecutorService> _logger;

    public CommandExecutorService(ILogger<CommandExecutorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executa um comando do sistema de forma assíncrona
    /// </summary>
    public async Task<CommandExecutionResult> ExecuteCommandAsync(string command)
    {
        if (!ErrorHandlingHelper.ValidateStringParameter(command, "command", _logger))
        {
            return CommandExecutionResult.Failure("Comando não pode ser vazio");
        }

        _logger.LogInformation("Executando comando: {Command}", command);
        
        var startInfo = ProcessHelper.CreateCmdProcessStartInfo(command);
        return await ExecuteProcessAsync(startInfo, "comando", command);
    }

    /// <summary>
    /// Executa um comando PowerShell
    /// </summary>
    public async Task<CommandExecutionResult> ExecutePowerShellAsync(string script)
    {
        if (!ErrorHandlingHelper.ValidateStringParameter(script, "script", _logger))
        {
            return CommandExecutionResult.Failure("Script PowerShell não pode ser vazio");
        }

        _logger.LogInformation("Executando script PowerShell: {Script}", script);
        
        var startInfo = ProcessHelper.CreatePowerShellProcessStartInfo(script);
        return await ExecuteProcessAsync(startInfo, "script PowerShell", script);
    }

    /// <summary>
    /// Método privado para executar processos de forma padronizada
    /// </summary>
    private async Task<CommandExecutionResult> ExecuteProcessAsync(ProcessStartInfo startInfo, string processType, string command)
    {
        return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
            async () =>
            {
                var result = await ProcessHelper.ExecuteProcessAsync(startInfo);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("{ProcessType} executado com sucesso. Exit code: {ExitCode}", processType, result.ExitCode);
                    return CommandExecutionResult.Success(result.Output);
                }
                else
                {
                    _logger.LogWarning("{ProcessType} falhou. Exit code: {ExitCode}, Error: {Error}", processType, result.ExitCode, result.Error);
                    return CommandExecutionResult.Failure(result.Error, result.Output);
                }
            },
            _logger,
            CommandExecutionResult.Failure($"Erro interno ao executar {processType}"),
            $"Erro ao executar {processType}: {command}"
        );
    }
}

/// <summary>
/// Resultado da execução de um comando
/// </summary>
public class CommandExecutionResult
{
    public string Output { get; }
    public string Error { get; }
    public DateTime ExecutionTime { get; }
    public bool IsSuccess { get; }

    private CommandExecutionResult(bool isSuccess, string output, string error)
    {
        IsSuccess = isSuccess;
        Output = output;
        Error = error;
        ExecutionTime = DateTime.UtcNow;
    }

    public static CommandExecutionResult Success(string output)
    {
        return new CommandExecutionResult(true, output, string.Empty);
    }

    public static CommandExecutionResult Failure(string error, string output = "")
    {
        return new CommandExecutionResult(false, output, error);
    }
} 