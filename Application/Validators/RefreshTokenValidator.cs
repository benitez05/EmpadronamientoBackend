using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Validators;

/// <summary>
/// Validador para la solicitud de renovación de token.
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        // Detiene la validación al primer error encontrado en el campo
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido para renovar el token.")
            .EmailAddress().WithMessage("El formato del correo no es válido.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El refresh token es obligatorio.")
            .MinimumLength(20).WithMessage("El refresh token proporcionado no es válido.");
    }
}