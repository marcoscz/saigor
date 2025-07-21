// <copyright file="IValidationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Saigor.Services
{
    using FluentValidation.Results;
    using Saigor.Models;

    /// <summary>
    /// Interface para serviços de validação de entidades do sistema.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Valida um job.
        /// </summary>
        /// <param name="job">O job a ser validado.</param>
        /// <returns>O resultado da validação.</returns>
        ValidationResult ValidateJob(JobModel job);

        /// <summary>
        /// Valida uma tarefa.
        /// </summary>
        /// <param name="tarefa">A tarefa a ser validada.</param>
        /// <returns>O resultado da validação.</returns>
        ValidationResult ValidateTarefa(TarefaModel tarefa);

        /// <summary>
        /// Valida uma conexão.
        /// </summary>
        /// <param name="conexao">A conexão a ser validada.</param>
        /// <returns>O resultado da validação.</returns>
        ValidationResult ValidateConexao(ConexaoModel conexao);
    }
} 