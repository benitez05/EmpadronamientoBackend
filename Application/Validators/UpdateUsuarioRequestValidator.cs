using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

public class UpdateUsuarioRequestValidator : AbstractValidator<UpdateUsuarioRequest>
{
    public UpdateUsuarioRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(100).WithMessage("Los apellidos no pueden exceder los 100 caracteres.");

        RuleFor(x => x.Celular)
            .Matches(@"^\d{10,15}$")
            .When(x => !string.IsNullOrEmpty(x.Celular))
            .WithMessage("El formato del celular no es válido.");

        // Validación de imagen (opcional)
        RuleFor(x => x.Imagen)
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
            .WithMessage("La imagen no puede superar los 5MB.");

        RuleFor(x => x.Imagen)
            .Must(file =>
            {
                if (file == null) return true;

                var allowedTypes = new[]
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };

                return allowedTypes.Contains(file.ContentType);
            })
            .WithMessage("La imagen debe ser JPG, PNG o WEBP.");
    }
}