using BenitezLabs.Domain.Entities;
namespace BenitezLabs.Domain.Entities;


/// <summary>
/// Representa un usuario dentro del sistema.
/// Entidad principal de autenticación.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public required string Correo { get; set; }
    public required string PasswordHash { get; set; }

    public string? Celular { get; set; }
    public string? Imagen { get; set; }

    // Refresh Tokens
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public DateTime? RefreshTokenCreated { get; set; }

    // Seguridad
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime? UltimoLogin { get; set; }

    // Confirmación
    public bool CorreoConfirmado { get; set; }
    public string? TokenConfirmacionCorreo { get; set; }

    // Auditoría
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}