using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

public class OrganizacionModulo : AuditoriaEntidad // <--- Heredamos auditoría completa
{
    public int OrganizacionId { get; set; }
    public virtual Organizacion Organizacion { get; set; } = null!;

    public int ModuloId { get; set; }
    public virtual Modulo Modulo { get; set; } = null!;

    // --- CONTROL DE ESTADO ---
    public bool Activo { get; set; } = true;
    public DateTime FechaActivacion { get; set; } = DateTime.UtcNow;
}