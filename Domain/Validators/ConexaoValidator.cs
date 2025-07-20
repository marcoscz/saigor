using FluentValidation;
using Saigor.Models;

namespace Saigor.Domain.Validators;

/// <summary>
/// Validador para a entidade ConexaoModel usando FluentValidation
/// </summary>
public class ConexaoValidator : AbstractValidator<ConexaoModel>
{
    public ConexaoValidator()
    {
        RuleFor(x => x.Conector)
            .NotEmpty().WithMessage("Conector é obrigatório")
            .MaximumLength(50).WithMessage("Conector não pode ter mais que 50 caracteres")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Conector deve conter apenas letras, números, hífens e underscores");

        RuleFor(x => x.Servidor)
            .NotEmpty().WithMessage("Servidor é obrigatório")
            .MaximumLength(200).WithMessage("Servidor não pode ter mais que 200 caracteres")
            .Must(BeValidServerAddress).WithMessage("Endereço do servidor deve ser válido");

        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente é obrigatório")
            .MaximumLength(50).WithMessage("Ambiente não pode ter mais que 50 caracteres")
            .Must(BeValidEnvironment).WithMessage("Ambiente deve ser: Produção, Homologação, Desenvolvimento ou Teste");

        RuleFor(x => x.Nome)
            .MaximumLength(100).WithMessage("Nome não pode ter mais que 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Nome));

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descrição não pode ter mais que 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }

    private static bool BeValidServerAddress(string serverAddress)
    {
        if (string.IsNullOrWhiteSpace(serverAddress))
            return false;

        // Aceita IP, hostname ou URL
        return Uri.TryCreate($"http://{serverAddress}", UriKind.Absolute, out _) ||
               System.Net.IPAddress.TryParse(serverAddress, out _) ||
               serverAddress.Contains('.') || serverAddress.Contains(':');
    }

    private static bool BeValidEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
            return false;

        var validEnvironments = new[] { "Produção", "Homologação", "Desenvolvimento", "Teste" };
        return validEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase);
    }
} 