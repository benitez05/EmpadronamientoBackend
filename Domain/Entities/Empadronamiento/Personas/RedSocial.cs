using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Redes sociales asociadas a una persona.
/// </summary>
public class RedSocial : AuditoriaEntidad
{
    public int Id { get; set; }

    /// <summary>
    /// Tipo de red social (Catálogo).
    /// </summary>
    public int TipoRedSocialId { get; set; }

    public string TipoRedSocialNombre { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string? UrlPerfil { get; set; }

    public int PersonaId { get; set; }

    public virtual Persona Persona { get; set; } = null!;

    public int OrganizacionId { get; set; }
}