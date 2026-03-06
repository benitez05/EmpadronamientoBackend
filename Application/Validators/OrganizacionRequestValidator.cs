using FluentValidation;

namespace EmpadronamientoBackend.Application.Validators;

public class OrganizacionRequestValidator : AbstractValidator<OrganizacionRequest>
{
    public OrganizacionRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder los 150 caracteres.");

        RuleFor(x => x.EmailContacto)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.EmailContacto))
            .WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.Telefono)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("El formato del teléfono no es válido (ej: +521234567890).");

        RuleFor(x => x.LogoUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("La URL del logo no es válida.");

        RuleFor(x => x.FechaVencimiento)
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha de vencimiento debe ser mayor a la fecha actual.");

        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Debes seleccionar un plan válido.");
    }
}