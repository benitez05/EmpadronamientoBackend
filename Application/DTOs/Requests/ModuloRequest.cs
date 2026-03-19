using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class ModuloRequest
{
    [Required]
    public string Nombre { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string K { get; set; } = null!;

    public string Color { get; set; } = "#6B7280";

    // Para subir un archivo físico (PNG/SVG)
    public IFormFile? IconoArchivo { get; set; }


}