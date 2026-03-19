using EmpadronamientoBackend.Application.DTOs.Requests;
using FluentValidation;
namespace EmpadronamientoBackend.Application.Validators;

public class CrearRedSocialValidator : AbstractValidator<CrearRedSocialRequest>
{
    public CrearRedSocialValidator()
    {
        RuleFor(x => x.TipoRedSocial)
            .NotEmpty();
    }
}