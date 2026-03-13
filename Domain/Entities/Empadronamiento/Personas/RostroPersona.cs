using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities;

/// <summary>
/// Representa un rostro registrado para una persona dentro del sistema.
///
/// Esta entidad está diseñada para integrarse con sistemas de reconocimiento
/// facial como Amazon Rekognition u otros motores biométricos.
///
/// Permite almacenar múltiples rostros por persona, registrar la imagen
/// utilizada y mantener identificadores externos generados por servicios
/// de reconocimiento facial.
/// </summary>
public class RostroPersona : AuditoriaEntidad
{
    /// <summary>
    /// Identificador único del registro de rostro.
    /// </summary>
    [Key]
    public int IdRostro { get; set; }

    /// <summary>
    /// Identificador de la persona a la que pertenece el rostro.
    /// </summary>
    public required int PersonaId { get; set; }

    /// <summary>
    /// Persona asociada al rostro.
    /// </summary>
    [ForeignKey(nameof(PersonaId))]
    public virtual Persona Persona { get; set; } = null!;

    /// <summary>
    /// URL o URI donde se almacena la imagen facial.
    /// Puede ser un storage como S3, CDN o servidor local.
    /// </summary>
    [MaxLength(500)]
    public required string ImageUri { get; set; }

    /// <summary>
    /// Identificador único del rostro generado por el sistema
    /// de reconocimiento facial (por ejemplo Amazon Rekognition).
    /// </summary>
    [MaxLength(200)]
    public required string FaceId { get; set; }

    /// <summary>
    /// Identificador de la imagen dentro del sistema de reconocimiento.
    /// Permite agrupar múltiples rostros provenientes de una misma imagen.
    /// </summary>
    [MaxLength(200)]
    public required string ImageId { get; set; }

    /// <summary>
    /// Nivel de confianza detectado por el sistema de reconocimiento.
    /// Usualmente es un valor entre 0 y 100.
    /// </summary>
    public decimal? Confianza { get; set; }

    /// <summary>
    /// Ancho del bounding box detectado del rostro dentro de la imagen.
    /// </summary>
    public decimal? BoundingBoxWidth { get; set; }

    /// <summary>
    /// Alto del bounding box detectado del rostro dentro de la imagen.
    /// </summary>
    public decimal? BoundingBoxHeight { get; set; }

    /// <summary>
    /// Coordenada X del rostro detectado dentro de la imagen.
    /// </summary>
    public decimal? BoundingBoxLeft { get; set; }

    /// <summary>
    /// Coordenada Y del rostro detectado dentro de la imagen.
    /// </summary>
    public decimal? BoundingBoxTop { get; set; }

    /// <summary>
    /// Organización propietaria del registro.
    /// Permite separar los datos en entornos multi-tenant.
    /// </summary>
    public int OrganizacionId { get; set; }

    public virtual Organizacion Organizacion { get; set; } = null!;
}