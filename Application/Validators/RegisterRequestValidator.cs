using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido")
            .EmailAddress().WithMessage("El formato del correo no es válido")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("El correo debe tener un dominio válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("Mínimo 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe contener al menos una minúscula")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número")
            .Matches(@"[\!\?\*\.\@\#\$\%\^]").WithMessage("Debe incluir al menos un carácter especial (!?*.@#$%).");
    }
}