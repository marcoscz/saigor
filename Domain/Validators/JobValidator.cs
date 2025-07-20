using FluentValidation;
using Saigor.Models;

namespace Saigor.Domain.Validators;

/// <summary>
/// Validador para a entidade Job usando FluentValidation
/// </summary>
public class JobValidator : AbstractValidator<JobModel>
{
    public JobValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome não pode ter mais que 100 caracteres")
            .Matches(@"^[a-zA-Z0-9 _-]+$").WithMessage("Nome deve conter apenas letras, números, espaços, hífens e underscores");

        RuleFor(x => x.Command)
            .NotEmpty().WithMessage("Comando é obrigatório")
            .MaximumLength(500).WithMessage("Comando não pode ter mais que 500 caracteres");

        RuleFor(x => x.Schedule)
            .NotEmpty().WithMessage("Agendamento é obrigatório")
            .MaximumLength(100).WithMessage("Agendamento não pode ter mais que 100 caracteres")
            .Must(BeValidCronExpression).WithMessage("Agendamento deve ser uma expressão CRON válida");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status deve ser um valor válido");
    }

    private static bool BeValidCronExpression(string cronExpression)
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
} 