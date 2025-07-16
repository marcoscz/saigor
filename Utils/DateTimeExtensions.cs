using System;
using System.Globalization;

namespace Saigor.Utils
{
    public static class DateTimeExtensions
    {
        public static string ToBrasiliaTimeString(this DateTime dateTime)
        {
            var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
#if WINDOWS
                "E. South America Standard Time"
#else
                "America/Sao_Paulo"
#endif
            );
            DateTime brasiliaTime;
            if (dateTime.Kind == DateTimeKind.Utc || dateTime.Kind == DateTimeKind.Unspecified)
            {
                // Trata Unspecified como UTC (se o banco está em UTC)
                brasiliaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), brasiliaTimeZone);
            }
            else
            {
                brasiliaTime = dateTime;
            }
            return brasiliaTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("pt-BR"));
        }
    }
}