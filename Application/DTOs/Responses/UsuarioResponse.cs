namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class UsuarioResponse
{
    public int Id { get; set; }
    public required string Correo { get; set; }
    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }
    public string? Celular { get; set; }
    public required int Tipo { get; set; }
    public RoleResponse? Rol { get; set; }


}