using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities.Catalogos;

/// <summary>
/// Representa un elemento dentro de un catálogo.
///
/// Ejemplo:
/// Catálogo: TIPO_FOTO
///
/// Items:
/// - Cara
/// - Cuerpo Completo
/// - Identificación
/// </summary>
public class CatalogoItem : AuditoriaEntidad
{
    /// <summary>
    /// Identificador del item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del elemento.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Código técnico opcional.
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Orden de visualización.
    /// </summary>
    public int Orden { get; set; }

    /// <summary>
    /// Indica si el item está activo.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Catálogo al que pertenece el item.
    /// </summary>
    public int CatalogoId { get; set; }

    public virtual Catalogo Catalogo { get; set; } = null!;

    /// <summary>
    /// Organización propietaria del catálogo.
    /// </summary>
    public int OrganizacionId { get; set; }

    public virtual Organizacion Organizacion { get; set; } = null!;
}