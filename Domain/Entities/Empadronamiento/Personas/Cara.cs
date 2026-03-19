using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BenitezLabs.Domain.Common;
using BenitezLabs.Domain.Entities;
namespace BenitezLabs.Domain.Entities;
[Table("Caras")]
public class Cara : AuditoriaEntidad
{
    [Key]
    public int Id { get; set; }
    
    // Relación obligatoria
    public int IdFoto { get; set; }
    
    // Para filtrar rápido por cliente sin hacer JOINs pesados
    public int OrganizacionId { get; set; }

    [Required, MaxLength(100)]
    public string FaceId { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string S3Key { get; set; } = string.Empty;

    public float Confidence { get; set; }

    // ¡Aquí está tu BoundingBox, no me pegues!
    public string? BoundingBox { get; set; }

    [ForeignKey(nameof(IdFoto))]
    public virtual Foto? Foto { get; set; }
}