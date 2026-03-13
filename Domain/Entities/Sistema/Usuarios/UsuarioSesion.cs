namespace BenitezLabs.Domain.Entities;

public class UsuarioSesion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    
    // Identificadores de seguridad
    public string Jti { get; set; } = string.Empty; // El ID único del JWT que explicamos
    public string RefreshToken { get; set; } = string.Empty;
    
    // Info para que el cliente vea en su lista
    public string DeviceInfo { get; set; } = string.Empty; // "Chrome en Windows", "iPhone 15"
    public string IpAddress { get; set; } = string.Empty;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public DateTime? UltimaActividad { get; set; }

    // Propiedad de navegación
    public virtual Usuario Usuario { get; set; } = null!;

    // El ID denormalizado para el filtro global
    public int OrganizacionId { get; set; }
}