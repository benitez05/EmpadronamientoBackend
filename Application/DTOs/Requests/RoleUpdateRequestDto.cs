using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RoleUpdateRequestDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = null!;
}