namespace EmpadronamientoBackend.Application.DTOs;

public record OrganizacionResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    string? EmailContacto,
    string? Telefono,
    string? Pais,
    string? Ciudad,
    string? LogoUrl,
    bool Activa,
    DateTime FechaVencimiento,
    string? NombrePlan
);