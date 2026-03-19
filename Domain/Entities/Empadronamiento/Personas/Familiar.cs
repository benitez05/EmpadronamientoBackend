using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Familiares relacionados con una persona.
/// </summary>
public class Familiar : AuditoriaEntidad
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string? Parentesco { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public int PersonaId { get; set; }

    public virtual Persona Persona { get; set; } = null!;

    public int OrganizacionId { get; set; }
}