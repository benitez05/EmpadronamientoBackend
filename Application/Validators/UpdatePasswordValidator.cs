using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Validators;

/// <summary>
/// Validador para el cambio de contraseña.
/// Asegura que la nueva clave cumpla con los estándares de seguridad de BenitezLabs.
/// </summary>
public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        // Detenemos en el primer error para no saturar al usuario
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("Debe contener al menos una letra minúscula.")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número.")
            .Matches(@"[\!\?\*\.\@\#\$\%\^]").WithMessage("Debe incluir al menos un carácter especial (!?*.@#$%).");
    }
}