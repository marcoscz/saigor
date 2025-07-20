using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para validações genéricas e padronizadas.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida se uma string não está vazia ou nula.
        /// </summary>
        public static bool IsValidString(string? value, int minLength = 1, int maxLength = 255)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            
            return value.Length >= minLength && value.Length <= maxLength;
        }

        /// <summary>
        /// Valida se um email é válido.
        /// </summary>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida se uma URL é válida.
        /// </summary>
        public static bool IsValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Valida se um número está dentro de um intervalo.
        /// </summary>
        public static bool IsValidNumber(int value, int min = int.MinValue, int max = int.MaxValue)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Valida se uma data é válida e está no passado.
        /// </summary>
        public static bool IsValidPastDate(DateTime date)
        {
            return date <= DateTime.Now;
        }

        /// <summary>
        /// Valida se uma data é válida e está no futuro.
        /// </summary>
        public static bool IsValidFutureDate(DateTime date)
        {
            return date > DateTime.Now;
        }

        /// <summary>
        /// Valida se uma expressão CRON é válida.
        /// </summary>
        public static bool IsValidCronExpression(string? cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            try
            {
                Quartz.CronExpression.ValidateExpression(cronExpression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida se um comando é seguro para execução.
        /// </summary>
        public static bool IsSafeCommand(string? command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            // Lista de comandos perigosos que devem ser bloqueados
            var dangerousCommands = new[]
            {
                "format", "del", "rm", "rmdir", "rd", "erase", "delete",
                "shutdown", "restart", "reboot", "halt", "poweroff",
                "net user", "net localgroup", "wmic", "reg", "regedit",
                "taskkill", "tasklist", "sc", "services.msc", "msconfig",
                "diskpart", "chkdsk", "sfc", "dism", "bcdedit", "bootrec"
            };

            var commandLower = command.ToLowerInvariant();
            return !dangerousCommands.Any(dangerous => commandLower.Contains(dangerous));
        }

        /// <summary>
        /// Valida se um nome de arquivo é válido.
        /// </summary>
        public static bool IsValidFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Caracteres inválidos em nomes de arquivo
            var invalidChars = Path.GetInvalidFileNameChars();
            return !fileName.Any(c => invalidChars.Contains(c));
        }

        /// <summary>
        /// Valida se um caminho é válido.
        /// </summary>
        public static bool IsValidPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida se uma string contém apenas caracteres alfanuméricos.
        /// </summary>
        public static bool IsAlphanumeric(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.All(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Valida se uma string contém apenas números.
        /// </summary>
        public static bool IsNumeric(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.All(char.IsDigit);
        }

        /// <summary>
        /// Valida se uma string tem o comprimento correto.
        /// </summary>
        public static bool HasValidLength(string? value, int minLength, int maxLength)
        {
            if (value == null)
                return minLength == 0;

            return value.Length >= minLength && value.Length <= maxLength;
        }

        /// <summary>
        /// Valida se uma string segue um padrão regex.
        /// </summary>
        public static bool MatchesPattern(string? value, string pattern)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(pattern))
                return false;

            try
            {
                return Regex.IsMatch(value, pattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida se um objeto não é nulo.
        /// </summary>
        public static bool IsNotNull(object? value)
        {
            return value != null;
        }

        /// <summary>
        /// Valida se uma coleção não está vazia.
        /// </summary>
        public static bool IsNotEmpty<T>(IEnumerable<T>? collection)
        {
            return collection != null && collection.Any();
        }

        /// <summary>
        /// Valida se um GUID é válido.
        /// </summary>
        public static bool IsValidGuid(string? guidString)
        {
            if (string.IsNullOrWhiteSpace(guidString))
                return false;

            return Guid.TryParse(guidString, out _);
        }

        /// <summary>
        /// Valida se um IP é válido.
        /// </summary>
        public static bool IsValidIpAddress(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Valida se uma porta é válida.
        /// </summary>
        public static bool IsValidPort(int port)
        {
            return port >= 1 && port <= 65535;
        }
    }
} 