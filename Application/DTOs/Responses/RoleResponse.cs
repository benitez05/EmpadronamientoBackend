namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class RoleResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    
    // Opcional: Lista de permisos simplificada para el front
    public List<ModuloResponse>? Permisos { get; set; }
}