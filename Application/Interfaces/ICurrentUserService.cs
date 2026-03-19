namespace EmpadronamientoBackend.Application.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    string Jti { get; }
    bool IsAuthenticated { get; }

    int Tipo { get; }
    int OrganizacionId { get; }
    string? IpAddress { get; }
    string? Dispositivo { get; }
}