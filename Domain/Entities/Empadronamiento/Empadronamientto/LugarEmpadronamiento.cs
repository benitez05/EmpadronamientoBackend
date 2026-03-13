using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities.Empadronamientos;

/// <summary>
/// Ubicación geográfica donde se realiza
/// un empadronamiento.
/// </summary>
public class LugarEmpadronamiento : AuditoriaEntidad
{
    public int Id { get; set; }

    /// <summary>
    /// Dirección del lugar.
    /// </summary>
    public string Calle { get; set; } = string.Empty;

    public string NumeroExterior { get; set; } = string.Empty;

    public string NumeroInterior { get; set; } = string.Empty;

    public int CP { get; set; } 

    public string? Colonia { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    /// <summary>
    /// Referencia adicional del lugar.
    /// </summary>
    public string? Referencia { get; set; }

    /// <summary>
    /// Coordenada geográfica latitud.
    /// </summary>
    public decimal? Latitud { get; set; }

    /// <summary>
    /// Coordenada geográfica longitud.
    /// </summary>
    public decimal? Longitud { get; set; }

    /// <summary>
    /// Imagen del lugar de los hechos.
    /// </summary>
    public string? ImagenUrl { get; set; }

    public int OrganizacionId { get; set; }

    public virtual Organizacion Organizacion { get; set; } = null!;
}