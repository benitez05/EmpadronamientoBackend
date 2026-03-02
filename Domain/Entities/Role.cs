namespace BenitezLabs.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public virtual ICollection<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
    public virtual ICollection<RolePermiso> Permisos { get; set; } = new HashSet<RolePermiso>();
}