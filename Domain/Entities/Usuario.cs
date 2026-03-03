namespace BenitezLabs.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public required string Correo { get; set; }
    public required string PasswordHash { get; set; }

    public string? Celular { get; set; }
    public string? Imagen { get; set; }

    // --- CONFIRMACIÓN (Los que me reclamaste, aquí están de vuelta) ---
    public bool CorreoConfirmado { get; set; }
    public string? TokenConfirmacionCorreo { get; set; }

    // --- SEGURIDAD ---
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime? UltimoLogin { get; set; }

    // --- AUDITORÍA ---
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // --- RELACIONES ---
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    // La colección para manejar múltiples dispositivos (Sesiones Remotas)
    public virtual ICollection<UsuarioSesion> Sesiones { get; set; } = new List<UsuarioSesion>();
}