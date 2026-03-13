using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Representa una persona registrada en el sistema.
/// 
/// Puede aparecer en múltiples empadronamientos
/// y tener múltiples direcciones, familiares,
/// redes sociales y fotografías.
/// </summary>
public class Persona : AuditoriaEntidad
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string ApellidoPaterno { get; set; } = string.Empty;

    public string ApellidoMaterno { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }

    public int? Edad { get; set; }

    public decimal? Estatura { get; set; }

    public string? Sexo { get; set; }

    public string? Originario { get; set; }

    public string? Telefono { get; set; }

    public string? Apodo { get; set; }

    public string? Nacionalidad { get; set; }

    /// <summary>
    /// Estado civil (Catálogo).
    /// </summary>
    public int? EstadoCivilId { get; set; }

    public string? EstadoCivilNombre { get; set; }

    /// <summary>
    /// Escolaridad (Catálogo).
    /// </summary>
    public int? EscolaridadId { get; set; }

    public string? EscolaridadNombre { get; set; }

    public string? OficioProfesion { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public int OrganizacionId { get; set; }

    /// <summary>
    /// Rostros registrados para la persona.
    /// </summary>
    public virtual ICollection<RostroPersona> Rostros { get; set; } = new List<RostroPersona>();

    public virtual Organizacion Organizacion { get; set; } = null!;

    public virtual ICollection<DireccionPersona> Direcciones { get; set; } = new List<DireccionPersona>();

    public virtual ICollection<FotoPersona> Fotos { get; set; } = new List<FotoPersona>();

    public virtual ICollection<RedSocial> RedesSociales { get; set; } = new List<RedSocial>();

    public virtual ICollection<Familiar> Familiares { get; set; } = new List<Familiar>();

    public virtual ICollection<EmpadronamientoPersona> Empadronamientos { get; set; } = new List<EmpadronamientoPersona>();
}