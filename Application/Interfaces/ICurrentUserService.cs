namespace EmpadronamientoBackend.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Jti { get; } // Agregamos esto para saber qué folio de sesión trae
    bool IsAuthenticated { get; }
}