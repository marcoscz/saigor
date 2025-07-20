using Saigor.Models;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para centralizar métodos de filtro comuns.
    /// </summary>
    public static class FilterHelper
    {
        /// <summary>
        /// Filtra jobs baseado em uma string de busca.
        /// </summary>
        /// <param name="job">Job a ser filtrado</param>
        /// <param name="searchString">String de busca</param>
        /// <returns>True se o job deve ser incluído no filtro</returns>
        public static bool FilterJob(JobModel job, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return job.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   job.Command.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   job.Schedule.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   job.Status.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Filtra logs baseado em uma string de busca.
        /// </summary>
        /// <param name="log">Log a ser filtrado</param>
        /// <param name="searchString">String de busca</param>
        /// <returns>True se o log deve ser incluído no filtro</returns>
        public static bool FilterLog(LogModel log, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return log.Job?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                   log.Status?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                   log.Output?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Filtra tarefas baseado em uma string de busca.
        /// </summary>
        /// <param name="tarefa">Tarefa a ser filtrada</param>
        /// <param name="searchString">String de busca</param>
        /// <returns>True se a tarefa deve ser incluída no filtro</returns>
        public static bool FilterTarefa(TarefaModel tarefa, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return tarefa.Nome.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   tarefa.Funcao.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   tarefa.Status.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Filtra conexões baseado em uma string de busca.
        /// </summary>
        /// <param name="conexao">Conexão a ser filtrada</param>
        /// <param name="searchString">String de busca</param>
        /// <returns>True se a conexão deve ser incluída no filtro</returns>
        public static bool FilterConexao(ConexaoModel conexao, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return (conexao.Nome?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
                   conexao.Conector.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   conexao.Servidor.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                   conexao.Ambiente.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Filtra uma lista genérica baseado em uma string de busca.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade</typeparam>
        /// <param name="item">Item a ser filtrado</param>
        /// <param name="searchString">String de busca</param>
        /// <param name="searchProperties">Funções para extrair propriedades para busca</param>
        /// <returns>True se o item deve ser incluído no filtro</returns>
        public static bool FilterGeneric<T>(T item, string searchString, params Func<T, string?>[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return searchProperties.Any(prop => 
                prop(item)?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
} 