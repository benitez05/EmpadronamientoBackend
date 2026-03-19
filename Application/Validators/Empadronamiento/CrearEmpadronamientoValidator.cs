using EmpadronamientoBackend.Application.DTOs.Requests;
using FluentValidation;

namespace EmpadronamientoBackend.Application.Validators;
public class CrearEmpadronamientoValidator : AbstractValidator<CrearEmpadronamientoRequest>
{
    public CrearEmpadronamientoValidator()
    {
        RuleFor(x => x.Lugar)
            .NotNull().WithMessage("El lugar es obligatorio");

        RuleFor(x => x.Personas)
            .NotEmpty().WithMessage("Debe haber al menos una persona");

        RuleForEach(x => x.Personas)
            .SetValidator(new CrearPersonaValidator());
    }
}