using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Validations;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // 1. VALIDACIÓN DE NOMBRE Y APELLIDOS
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre es demasiado largo");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(100).WithMessage("Los apellidos son demasiado largos");

        // 2. VALIDACIÓN DE CORREO (Igual que el Login)
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido")
            .EmailAddress().WithMessage("El formato del correo no es válido")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("El correo debe tener un dominio válido");

        // 3. VALIDACIÓN DE CONTRASEÑA (Reglas de Seguridad BenitezLabs)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una letra mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe contener al menos una letra minúscula")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número")
            // La regex [^a-zA-Z0-9] acepta %, $, #, @, !, ?, *, ., etc.
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial (ej: !?*.%#@)");
    }
}