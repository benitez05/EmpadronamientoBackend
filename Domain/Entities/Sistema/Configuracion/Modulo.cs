namespace BenitezLabs.Domain.Entities;

public class Modulo {
    public int Id { get; set; }
    public string Nombre { get; set; } = null!; // Ej: "Usuarios"
    
    /// <summary>
    /// Key corta para el JWT (ej: "u")
    /// </summary>
    public string K { get; set; } = null!; 

    public string? Icono { get; set; }

    public string Color { get; set; } = "#6B7280"; // Gris de UI por defecto

    public virtual ICollection<RolePermiso> RolePermisos { get; set; } = new HashSet<RolePermiso>();
    
    // NUEVO: Relación para saber qué organizaciones tienen este módulo
    public virtual ICollection<OrganizacionModulo> OrganizacionesConAcceso { get; set; } = new HashSet<OrganizacionModulo>();
}