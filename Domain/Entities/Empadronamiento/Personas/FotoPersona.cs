using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Fotografías asociadas a una persona.
/// 
/// Permite almacenar fotos de:
/// - rostro
/// - cuerpo completo
/// - identificación
/// </summary>
public class FotoPersona : AuditoriaEntidad
{
    public int Id { get; set; }

    /// <summary>
    /// URL donde se almacena la fotografía
    /// </summary>
    public string Url { get; set; } = string.Empty;

    public int TipoFotoId { get; set; }

    public string TipoFotoNombre { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public int PersonaId { get; set; }

    public virtual Persona Persona { get; set; } = null!;

    public int OrganizacionId { get; set; }
}