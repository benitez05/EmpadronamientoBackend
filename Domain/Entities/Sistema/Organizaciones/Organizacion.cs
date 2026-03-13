using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

public class Organizacion : AuditoriaEntidad 
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    
    // --- CONTACTO Y LOCALIZACIÓN ---
    public string? EmailContacto { get; set; }
    public string? Telefono { get; set; }
    public string Calle { get; set; } = string.Empty;

    public string NumeroExterior { get; set; } = string.Empty;

    public string NumeroInterior { get; set; } = string.Empty;

    public int CP { get; set; } 

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Pais { get; set; }

    // --- IDENTIDAD VISUAL ---
    public string? LogoUrl { get; set; }

    // --- CONTROL DE ACCESO Y ESTADO ---
    public bool Activa { get; set; } = true;
    public DateTime FechaVencimiento { get; set; }
    public string? MotivoSuspension { get; set; }

    // --- RELACIONES ---
    public int PlanId { get; set; }
    public virtual Plan Plan { get; set; } = null!;
    
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    // Módulos específicos que esta organización tiene permitidos
    public virtual ICollection<OrganizacionModulo> ModulosContratados { get; set; } = new HashSet<OrganizacionModulo>();
}