using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BenitezLabs.Domain.Common;
using BenitezLabs.Domain.Entities.Empadronamientos;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Representa una foto asociada a una persona y un proceso de empadronamiento.
/// </summary>
public class Foto
{
    [Key]
    public int Id { get; set; }

    public int? IdPersona { get; set; }
    public int? IdEmpadronamiento { get; set; }
    public int OrganizacionId { get; set; }

    [Required]
    [MaxLength(500)]
    public string S3Key { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Tipo { get; set; }

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [ForeignKey(nameof(IdPersona))]
    public virtual Persona? Persona { get; set; }

    [ForeignKey(nameof(IdEmpadronamiento))]
    public virtual Empadronamiento? Empadronamiento { get; set; }
}