using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RegisterRequest
{
    [DefaultValue("Roberto")]
    public required string Nombre { get; set; }

    [DefaultValue("Benitez")]
    public required string Apellidos { get; set; }

    [DefaultValue("admin@benitezlabs.com")]
    public required string Correo { get; set; }

    [DefaultValue("Admin123!")] // <-- ¡Aquí está tu password, mijo!
    public required string Password { get; set; }
}