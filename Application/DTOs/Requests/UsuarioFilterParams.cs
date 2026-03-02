namespace EmpadronamientoBackend.Application.DTOs.Requests;

/// <summary>
/// Parámetros de consulta para filtrar la lista de usuarios.
/// </summary>
public class UsuarioFilterParams
{
    public string? Busqueda { get; set; }
    public int? RoleId { get; set; }
    public bool? Activo { get; set; }
}