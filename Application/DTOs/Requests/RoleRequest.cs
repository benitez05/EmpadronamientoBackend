using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RoleRequest
{
    /// <summary>Nombre del rol o puesto</summary>
    [DefaultValue("Administrador")]
    public required string Nombre { get; set; }
}