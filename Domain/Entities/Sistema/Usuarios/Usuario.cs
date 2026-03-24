using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

public class Usuario : AuditoriaEntidad // <--- Heredamos toda la auditoría pro
{
    public int Id { get; set; }

    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public required string Correo { get; set; }
    public required string PasswordHash { get; set; }

    public string? Celular { get; set; }
    public string? Imagen { get; set; }

    // Bypass de seguridad: 3 es el Dios del sistema
    public int Tipo { get; set; } = 1;

    public int OrganizacionId { get; set; }
    public virtual Organizacion Organizacion { get; set; } = null!;

    // --- CONFIRMACIÓN ---
    public bool CorreoConfirmado { get; set; }
    public string? TokenConfirmacionCorreo { get; set; }

    // --- SEGURIDAD ---
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime? UltimoLogin { get; set; }

    // --- SEGURIDAD 2FA (TOTP) ---
    public bool DosPasosHabilitado { get; set; } = false;
    public string? DosPasosSecretKey { get; set; }

    // --- ESTADO ---
    public bool Activo { get; set; } = true;
    // Quitamos FechaCreacion y FechaActualizacion porque ya vienen en AuditoriaEntidad

    // --- RELACIONES ---
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    // La colección para manejar múltiples dispositivos (Sesiones Remotas)
    public virtual ICollection<UsuarioSesion> Sesiones { get; set; } = new List<UsuarioSesion>();
}