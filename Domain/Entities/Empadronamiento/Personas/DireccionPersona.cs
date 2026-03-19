using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Direcciones asociadas a una persona.
/// 
/// Una persona puede tener múltiples domicilios
/// registrados en el sistema.
/// </summary>
public class DireccionPersona : AuditoriaEntidad
{
    public int Id { get; set; }

    public string Calle { get; set; } = string.Empty;

    public string NumeroExterior { get; set; } = string.Empty;

    public string NumeroInterior { get; set; } = string.Empty;

    public int CP { get; set; } 

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Pais { get; set; }

    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public bool EsPrincipal { get; set; }

    public int PersonaId { get; set; }

    public virtual Persona Persona { get; set; } = null!;

    public int OrganizacionId { get; set; }
}