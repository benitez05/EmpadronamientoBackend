public record OrganizacionRequest(
    string Nombre,           // Obligatorio (HasMaxLength 150)
    string EmailContacto,    // <--- QUITAR EL '?' - Ahora es OBLIGATORIO según tu config
    int PlanId,              // Obligatorio
    DateTime FechaVencimiento,
    string? Descripcion,     // Opcional
    string? Telefono,        // Opcional (HasMaxLength 20)
    string? Direccion,
    string? Pais,
    string? Ciudad,
    string? LogoUrl,
    bool Activa = true
);