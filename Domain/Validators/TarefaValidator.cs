using FluentValidation;
using Saigor.Models;

namespace Saigor.Domain.Validators;

/// <summary>
/// Validador para a entidade TarefaModel usando FluentValidation
/// </summary>
public class TarefaValidator : AbstractValidator<TarefaModel>
{
    public TarefaValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da tarefa é obrigatório")
            .MaximumLength(100).WithMessage("Nome da tarefa não pode ter mais que 100 caracteres")
            .Matches(@"^[a-zA-Z0-9 _-]+$").WithMessage("Nome deve conter apenas letras, números, espaços, hífens e underscores");

        RuleFor(x => x.Funcao)
            .NotEmpty().WithMessage("Função da tarefa é obrigatória")
            .MaximumLength(200).WithMessage("Função da tarefa não pode ter mais que 200 caracteres")
            .Must(BeValidFunction).WithMessage("Função deve ser um nome de método válido");

        RuleFor(x => x.Parametros)
            .MaximumLength(1000).WithMessage("Parâmetros não podem ter mais que 1000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Parametros));

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status da tarefa é obrigatório")
            .MaximumLength(50).WithMessage("Status da tarefa não pode ter mais que 50 caracteres")
            .Must(BeValidStatus).WithMessage("Status deve ser: Pendente, Em Execução, Concluída, Falhou ou Cancelada");

        RuleFor(x => x.DataCriacao)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser no futuro");
    }

    private static bool BeValidFunction(string function)
    {
        if (string.IsNullOrWhiteSpace(function))
            return false;

        // Verifica se é um nome de método válido
        return function.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.') &&
               char.IsLetter(function[0]) &&
               !function.Contains("  ") && // Não pode ter espaços duplos
               function.Length >= 2;
    }

    private static bool BeValidStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var validStatuses = new[] { "Pendente", "Em Execução", "Concluída", "Falhou", "Cancelada" };
        return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
} 