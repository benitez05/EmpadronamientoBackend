using BenitezLabs.Domain.Common;

namespace BenitezLabs.Domain.Entities.Catalogos;

/// <summary>
/// Representa un catálogo configurable del sistema.
///
/// Los catálogos permiten definir listas reutilizables
/// utilizadas por múltiples entidades del sistema.
///
/// Ejemplos de catálogos:
/// - TIPO_RED_SOCIAL
/// - TIPO_FOTO
/// - ESTADO_CIVIL
/// - ESCOLARIDAD
/// - PARENTESCO
/// - TIPO_DOCUMENTO
/// - TIPO_TELEFONO
///
/// Los valores del catálogo se almacenan en la entidad
/// <see cref="CatalogoItem"/>.
/// </summary>
public class Catalogo : AuditoriaEntidad
{
    /// <summary>
    /// Identificador del catálogo.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Clave técnica del catálogo utilizada en código.
    /// Debe ser única.
    /// </summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo del catálogo.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción funcional del catálogo.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Organización propietaria del catálogo.
    /// </summary>
    public int OrganizacionId { get; set; }

    public virtual Organizacion Organizacion { get; set; } = null!;

    /// <summary>
    /// Items pertenecientes al catálogo.
    /// </summary>
    public virtual ICollection<CatalogoItem> Items { get; set; } = new List<CatalogoItem>();
}