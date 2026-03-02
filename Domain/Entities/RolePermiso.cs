namespace BenitezLabs.Domain.Entities;

public class RolePermiso {
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int ModuloId { get; set; }
    public virtual Modulo Modulo { get; set; } = null!;

    /// <summary>
    /// 1: Leer, 2: Escribir, 3: Borrar
    /// </summary>
    public int Lvl { get; set; }
}