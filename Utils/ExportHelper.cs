using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para exportação de dados em diferentes formatos.
    /// </summary>
    public static class ExportHelper
    {
        /// <summary>
        /// Exporta uma lista de objetos para CSV.
        /// </summary>
        /// <typeparam name="T">Tipo dos objetos</typeparam>
        /// <param name="items">Lista de itens</param>
        /// <param name="propertySelectors">Seletores de propriedades para exportar</param>
        /// <returns>String CSV</returns>
        public static string ExportToCsv<T>(
            IEnumerable<T> items, 
            params (string Header, Func<T, object> Selector)[] propertySelectors)
        {
            var csv = new StringBuilder();
            // Cabeçalho
            var headers = propertySelectors.Select(p => p.Header);
            csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));
            // Dados
            foreach (var item in items)
            {
                var values = propertySelectors.Select(p => p.Selector(item)?.ToString() ?? "");
                csv.AppendLine(string.Join(",", values.Select(v => $"\"{v.Replace("\"", "\"\"")}")));
            }
            return csv.ToString();
        }

        /// <summary>
        /// Exporta uma lista de objetos para JSON.
        /// </summary>
        /// <typeparam name="T">Tipo dos objetos</typeparam>
        /// <param name="items">Lista de itens</param>
        /// <returns>String JSON</returns>
        public static string ExportToJson<T>(IEnumerable<T> items)
        {
            return System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Gera um relatório HTML simples.
        /// </summary>
        /// <typeparam name="T">Tipo dos objetos</typeparam>
        /// <param name="items">Lista de itens</param>
        /// <param name="title">Título do relatório</param>
        /// <param name="propertySelectors">Seletores de propriedades</param>
        /// <returns>String HTML</returns>
        public static string ExportToHtml<T>(
            IEnumerable<T> items, 
            string title,
            params (string Header, Func<T, object> Selector)[] propertySelectors)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<title>" + title + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"<h1>{title}</h1>");
            html.AppendLine("<table>");
            // Cabeçalho
            html.AppendLine("<thead><tr>");
            foreach (var header in propertySelectors)
            {
                html.AppendLine($"<th>{header.Header}</th>");
            }
            html.AppendLine("</tr></thead>");
            // Dados
            html.AppendLine("<tbody>");
            foreach (var item in items)
            {
                html.AppendLine("<tr>");
                foreach (var selector in propertySelectors)
                {
                    var value = selector.Selector(item)?.ToString() ?? "";
                    html.AppendLine($"<td>{value}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        /// <summary>
        /// Gera estatísticas básicas de uma lista.
        /// </summary>
        /// <typeparam name="T">Tipo dos objetos</typeparam>
        /// <param name="items">Lista de itens</param>
        /// <param name="countSelector">Seletor para contagem</param>
        /// <returns>Dicionário com estatísticas</returns>
        public static Dictionary<string, object> GenerateStatistics<T>(
            IEnumerable<T> items,
            Func<T, bool>? countSelector = null)
        {
            var stats = new Dictionary<string, object>();
            var itemList = items.ToList();
            stats["Total"] = itemList.Count;
            stats["Última Atualização"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            if (countSelector != null)
            {
                stats["Filtrado"] = itemList.Count(countSelector);
            }
            return stats;
        }
    }
} 