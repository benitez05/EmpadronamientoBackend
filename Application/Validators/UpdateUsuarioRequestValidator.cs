using FluentValidation;
using EmpadronamientoBackend.Application.DTOs.Requests;

public class UpdateUsuarioRequestValidator : AbstractValidator<UpdateUsuarioRequest>
{
    public UpdateUsuarioRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Celular)
            .Matches(@"^\d{10,15}$")
            .When(x => !string.IsNullOrEmpty(x.Celular))
            .WithMessage("El formato del celular no es válido.");
        
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .When(x => x.RoleId.HasValue)
            .WithMessage("El ID de rol debe ser válido.");
    }
}