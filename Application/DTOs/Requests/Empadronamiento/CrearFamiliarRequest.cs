namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class CrearFamiliarRequest
{
    // 🔥 Se agrega el Id opcional para actualización
    public int? Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;
    public required string Parentesco { get; set; } // item catalogo id
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}