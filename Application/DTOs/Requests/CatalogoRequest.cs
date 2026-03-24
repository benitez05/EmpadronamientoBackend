using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class CreateCatalogoItemRequest
{
    [Required] public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public int Orden { get; set; }
}

public class UpdateCatalogoItemRequest
{
    [Required] public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
}