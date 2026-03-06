using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RegisterRequest
{
    [DefaultValue("Roberto")]
    public required string Nombre { get; set; }

    [DefaultValue("Benitez")]
    public required string Apellidos { get; set; }

    [DefaultValue("admin@benitezlabs.com")]
    public required string Correo { get; set; }

    [DefaultValue("Admin123!")] 
    public required string Password { get; set; }

    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un rol válido.")]
    public required int RoleId { get; set; } 
}