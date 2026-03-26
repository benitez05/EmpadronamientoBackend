namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class ModuloResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string K { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? Icono { get; set; }
    public bool Multinivel { get; set; } // Aquí sí puede ser bool normal
}