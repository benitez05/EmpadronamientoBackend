using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class ModuloRequest
{
    /// <summary>Nombre descriptivo del módulo</summary>
    [DefaultValue("Usuarios")]
    public required string Nombre { get; set; }

    /// <summary>Clave abreviada para el Token (JWT)</summary>
    [DefaultValue("u")]
    public required string K { get; set; }
}