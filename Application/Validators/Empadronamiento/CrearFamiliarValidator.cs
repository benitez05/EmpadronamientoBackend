using EmpadronamientoBackend.Application.DTOs.Requests;
using FluentValidation;
namespace EmpadronamientoBackend.Application.Validators;

public class CrearFamiliarValidator : AbstractValidator<CrearFamiliarRequest>
{
    public CrearFamiliarValidator()
    {
        RuleFor(x => x.NombreCompleto)
            .NotEmpty();

        RuleFor(x => x.Parentesco)
            .NotEmpty();
    }
}