using EmpadronamientoBackend.Application.DTOs.Requests;
using FluentValidation;
namespace EmpadronamientoBackend.Application.Validators;

public class CrearPersonaValidator : AbstractValidator<CrearPersonaEmpadronamientoRequest>
{
    public CrearPersonaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();

        RuleFor(x => x.Rostro)
            .NotNull().WithMessage("El rostro es obligatorio");

        RuleFor(x => x.Direccion)
            .NotNull().WithMessage("La dirección es obligatoria");

        RuleFor(x => x.Fotos)
            .NotEmpty().WithMessage("Debe tener al menos una foto");

        RuleForEach(x => x.Fotos)
            .SetValidator(new CrearFotoValidator());

        RuleFor(x => x.RedesSociales)
            .NotEmpty().WithMessage("Debe tener al menos una red social");

        RuleForEach(x => x.RedesSociales)
            .SetValidator(new CrearRedSocialValidator());

        RuleFor(x => x.Familiares)
            .NotEmpty().WithMessage("Debe tener al menos un familiar");

        RuleForEach(x => x.Familiares)
            .SetValidator(new CrearFamiliarValidator());
    }
}