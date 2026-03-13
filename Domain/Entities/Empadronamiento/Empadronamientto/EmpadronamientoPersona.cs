using BenitezLabs.Domain.Common;
using BenitezLabs.Domain.Entities.Empadronamientos;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Tabla intermedia que relaciona personas con empadronamientos.
/// 
/// Permite que un empadronamiento tenga múltiples personas
/// registradas durante el evento.
/// </summary>
public class EmpadronamientoPersona : AuditoriaEntidad
{
    public int EmpadronamientoId { get; set; }

    /// <summary>
    /// Empadronamiento donde se registró la persona
    /// </summary>
    public virtual Empadronamiento Empadronamiento { get; set; } = null!;

    public int PersonaId { get; set; }

    /// <summary>
    /// Persona registrada
    /// </summary>
    public virtual Persona Persona { get; set; } = null!;

    /// <summary>
    /// Observaciones específicas sobre la persona durante el evento
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Organización propietaria
    /// </summary>
    public int OrganizacionId { get; set; }
}