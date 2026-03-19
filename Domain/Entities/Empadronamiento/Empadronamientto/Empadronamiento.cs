using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities.Empadronamientos;

/// <summary>
/// Representa una ficha de empadronamiento realizada
/// por un agente durante una inspección o registro
/// preventivo.
///
/// Un empadronamiento puede registrar múltiples
/// personas involucradas en el evento.
/// </summary>
public class Empadronamiento : AuditoriaEntidad
{
    public int Id { get; set; }

    /// <summary>
    /// Fecha en la que se realiza el empadronamiento.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Hora del evento.
    /// </summary>
    public TimeSpan Hora { get; set; }

    /// <summary>
    /// Folio administrativo del registro.
    /// </summary>
    public string? Folio { get; set; }


     /// <summary>
    /// Número CRP del operativo.
    /// </summary>
    public string? CRP { get; set; }

    /// <summary>
    /// Narrativa de los hechos registrados por el agente.
    /// </summary>
    public string NarrativaHechos { get; set; } = string.Empty;

    /// <summary>
    /// Usuario del sistema responsable de crear el registro.
    /// </summary>
    public int UsuarioResponsableId { get; set; }

    public virtual Usuario UsuarioResponsable { get; set; } = null!;

    /// <summary>
    /// Lugar donde ocurrió el evento.
    /// </summary>
    public int LugarEmpadronamientoId { get; set; }

    public virtual LugarEmpadronamiento Lugar { get; set; } = null!;

    public int OrganizacionId { get; set; }

    public virtual Organizacion Organizacion { get; set; } = null!;

    /// <summary>
    /// Personas registradas durante el empadronamiento.
    /// </summary>
    public virtual ICollection<EmpadronamientoPersona> Personas { get; set; } = new List<EmpadronamientoPersona>();
}