using EmpadronamientoBackend.Application.DTOs.Requests;
using FluentValidation;
namespace EmpadronamientoBackend.Application.Validators;

public class CrearFotoValidator : AbstractValidator<CrearFotoRequest>
{
    public CrearFotoValidator()
    {
        RuleFor(x => x.FotoId)
            .NotNull().WithMessage("La foto es obligatoria");

        RuleFor(x => x.TipoFoto)
            .NotEmpty().WithMessage("El tipo de foto es obligatorio");
    }
}