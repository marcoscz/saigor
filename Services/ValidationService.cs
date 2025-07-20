using FluentValidation.Results;
using Saigor.Domain.Validators;
using Saigor.Models;
using Saigor.Utils;

namespace Saigor.Services;

/// <summary>
/// Serviço de validação usando FluentValidation
/// </summary>
public class ValidationService : IValidationService
{
    private readonly JobValidator _jobValidator;
    private readonly ConexaoValidator _conexaoValidator;
    private readonly TarefaValidator _tarefaValidator;

    public ValidationService()
    {
        _jobValidator = new JobValidator();
        _conexaoValidator = new ConexaoValidator();
        _tarefaValidator = new TarefaValidator();
    }

    /// <summary>
    /// Valida uma entidade Job
    /// </summary>
    public ValidationResult ValidateJob(JobModel job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        return _jobValidator.Validate(job);
    }

    /// <summary>
    /// Valida uma entidade ConexaoModel
    /// </summary>
    public ValidationResult ValidateConexao(ConexaoModel conexao)
    {
        if (conexao == null)
            throw new ArgumentNullException(nameof(conexao));

        return _conexaoValidator.Validate(conexao);
    }

    /// <summary>
    /// Valida uma entidade TarefaModel
    /// </summary>
    public ValidationResult ValidateTarefa(TarefaModel tarefa)
    {
        if (tarefa == null)
            throw new ArgumentNullException(nameof(tarefa));

        return _tarefaValidator.Validate(tarefa);
    }

    /// <summary>
    /// Verifica se uma expressão CRON é válida
    /// </summary>
    public bool IsValidCronExpression(string cronExpression)
    {
        return ValidationHelper.IsValidCronExpression(cronExpression);
    }

    /// <summary>
    /// Verifica se um comando é seguro para execução
    /// </summary>
    public bool IsSafeCommand(string command)
    {
        return ValidationHelper.IsSafeCommand(command);
    }

    /// <summary>
    /// Valida uma string não vazia
    /// </summary>
    public bool IsValidString(string? value, int maxLength = 100)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;
    }

    /// <summary>
    /// Valida um ID positivo
    /// </summary>
    public bool IsValidId(int id)
    {
        return id > 0;
    }

    /// <summary>
    /// Valida um email
    /// </summary>
    public bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida uma URL
    /// </summary>
    public bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}