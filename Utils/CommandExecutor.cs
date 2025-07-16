using System.Diagnostics;
using System.Threading.Tasks;

namespace Saigor.Utils
{
    public static class CommandExecutor
    {
        public static async Task<string> RunCommandAsync(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return string.IsNullOrWhiteSpace(error) ? output : output + "\n" + error;
        }
    }
} 