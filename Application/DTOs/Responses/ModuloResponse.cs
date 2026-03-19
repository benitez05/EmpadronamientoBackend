namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class ModuloResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string K { get; set; } = null!;
    public string? Icono { get; set; } // Aquí caerá la URL de S3 o el nombre del icono
    public string Color { get; set; } = null!;
}