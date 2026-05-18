using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RoleDeleteRequestDto
{
    [Required(ErrorMessage = "El ID del rol a eliminar es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del rol a eliminar debe ser un número válido.")]
    public int RoleIdAEliminar { get; set; }

    [Required(ErrorMessage = "El ID del rol sustituto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del rol sustituto debe ser un número válido.")]
    public int RoleIdSustituto { get; set; }
}