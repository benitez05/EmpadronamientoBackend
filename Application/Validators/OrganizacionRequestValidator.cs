using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Validators;

public class UpdateOrganizacionRequestValidator : AbstractValidator<OrganizacionRequest>
{
    public UpdateOrganizacionRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder los 150 caracteres.");

        RuleFor(x => x.EmailContacto)
            .NotEmpty().WithMessage("El correo de contacto es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.Telefono)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Telefono))
            .WithMessage("El formato del teléfono no es válido (ej: +521234567890).");

        RuleFor(x => x.PlanId)
            .GreaterThan(0)
            .WithMessage("Debes seleccionar un plan válido.");

        RuleFor(x => x.FechaVencimiento)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha de vencimiento debe ser mayor a la fecha actual.");

        // Validación del logo (archivo)
        RuleFor(x => x.Logo)
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
            .WithMessage("El logo no puede superar los 5MB.");

        RuleFor(x => x.Logo)
            .Must(file =>
            {
                if (file == null) return true;

                var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
                return allowed.Contains(file.ContentType);
            })
            .WithMessage("El logo debe ser una imagen válida (JPG, PNG o WEBP).");
    }
}