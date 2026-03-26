using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class ModuloRequest
{
    [Required(ErrorMessage = "El nombre del módulo es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La clave corta (K) es obligatoria.")]
    [MaxLength(10)]
    public string K { get; set; } = null!;

    public string Color { get; set; } = "#6B7280";

    // Para subir un archivo físico (PNG/SVG)
    public IFormFile? IconoArchivo { get; set; }

    // 🔥 AGREGADO: Propiedad Multinivel
    // Se pone como bool? para que el [Required] detecte si el front NO lo mandó.
    [Required(ErrorMessage = "Debes especificar si el módulo es Multinivel.")]
    public bool? Multinivel { get; set; }
}