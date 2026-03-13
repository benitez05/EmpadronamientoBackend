using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

public class Role : AuditoriaEntidad // <--- Heredamos toda la auditoría
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    // --- MULTI-TENANT ---
    // Cada empresa tiene sus propios roles, aislados de las demás.
    public int OrganizacionId { get; set; }
    public virtual Organizacion Organizacion { get; set; } = null!;

    // --- RELACIONES ---
    public virtual ICollection<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
    public virtual ICollection<RolePermiso> Permisos { get; set; } = new HashSet<RolePermiso>();
}