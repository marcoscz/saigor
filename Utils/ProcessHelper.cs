using System.Diagnostics;

namespace Saigor.Utils
{
    /// <summary>
    /// Helper para padronizar a configuração e execução de processos
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Cria uma configuração padrão para ProcessStartInfo
        /// </summary>
        /// <param name="fileName">Nome do arquivo executável</param>
        /// <param name="arguments">Argumentos do comando</param>
        /// <param name="workingDirectory">Diretório de trabalho (opcional)</param>
        /// <returns>ProcessStartInfo configurado</returns>
        public static ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments, string? workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            return startInfo;
        }

        /// <summary>
        /// Cria uma configuração para comando CMD
        /// </summary>
        /// <param name="command">Comando a ser executado</param>
        /// <param name="workingDirectory">Diretório de trabalho (opcional)</param>
        /// <returns>ProcessStartInfo configurado para CMD</returns>
        public static ProcessStartInfo CreateCmdProcessStartInfo(string command, string? workingDirectory = null)
        {
            return CreateProcessStartInfo("cmd.exe", $"/c {command}", workingDirectory);
        }

        /// <summary>
        /// Cria uma configuração para comando PowerShell
        /// </summary>
        /// <param name="script">Script PowerShell a ser executado</param>
        /// <param name="workingDirectory">Diretório de trabalho (opcional)</param>
        /// <returns>ProcessStartInfo configurado para PowerShell</returns>
        public static ProcessStartInfo CreatePowerShellProcessStartInfo(string script, string? workingDirectory = null)
        {
            return CreateProcessStartInfo("powershell.exe", $"-Command \"{script}\"", workingDirectory);
        }

        /// <summary>
        /// Configura handlers de output para um processo
        /// </summary>
        /// <param name="process">Processo a ser configurado</param>
        /// <param name="outputBuilder">StringBuilder para output padrão</param>
        /// <param name="errorBuilder">StringBuilder para output de erro</param>
        public static void ConfigureOutputHandlers(Process process, System.Text.StringBuilder outputBuilder, System.Text.StringBuilder errorBuilder)
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };
        }

        /// <summary>
        /// Executa um processo de forma assíncrona com timeout
        /// </summary>
        /// <param name="startInfo">Configuração do processo</param>
        /// <param name="timeout">Timeout em milissegundos (opcional)</param>
        /// <returns>Resultado da execução</returns>
        public static async Task<ProcessExecutionResult> ExecuteProcessAsync(ProcessStartInfo startInfo, int? timeout = null)
        {
            using var process = new Process { StartInfo = startInfo };
            var output = new System.Text.StringBuilder();
            var error = new System.Text.StringBuilder();

            ConfigureOutputHandlers(process, output, error);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (timeout.HasValue)
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout.Value));
                    await process.WaitForExitAsync(cts.Token);
                }
                else
                {
                    await process.WaitForExitAsync();
                }

                var success = process.ExitCode == 0;
                var resultOutput = output.ToString().Trim();
                var errorOutput = error.ToString().Trim();

                return new ProcessExecutionResult
                {
                    IsSuccess = success,
                    Output = resultOutput,
                    Error = errorOutput,
                    ExitCode = process.ExitCode,
                    ExecutionTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ProcessExecutionResult
                {
                    IsSuccess = false,
                    Output = output.ToString().Trim(),
                    Error = ex.Message,
                    ExitCode = -1,
                    ExecutionTime = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Verifica se um processo está em execução
        /// </summary>
        /// <param name="processName">Nome do processo</param>
        /// <returns>True se o processo está em execução</returns>
        public static bool IsProcessRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtém informações sobre um processo
        /// </summary>
        /// <param name="processName">Nome do processo</param>
        /// <returns>Informações do processo ou null se não encontrado</returns>
        public static ProcessInfo? GetProcessInfo(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                    return null;

                var process = processes[0];
                return new ProcessInfo
                {
                    Id = process.Id,
                    ProcessName = process.ProcessName,
                    StartTime = process.StartTime,
                    WorkingSet = process.WorkingSet64,
                    CpuTime = process.TotalProcessorTime,
                    ThreadCount = process.Threads.Count
                };
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Resultado da execução de um processo
    /// </summary>
    public class ProcessExecutionResult
    {
        public bool IsSuccess { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public DateTime ExecutionTime { get; set; }
    }

    /// <summary>
    /// Informações sobre um processo
    /// </summary>
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public long WorkingSet { get; set; }
        public TimeSpan CpuTime { get; set; }
        public int ThreadCount { get; set; }
    }
} 