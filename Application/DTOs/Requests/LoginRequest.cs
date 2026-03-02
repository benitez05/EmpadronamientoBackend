using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class LoginRequest
{
    /// <summary>Correo electrónico del usuario registrado</summary>
    [DefaultValue("admin@benitezlabs.com")]
    public required string Correo { get; set; }

    /// <summary>Contraseña de acceso</summary>
    [DefaultValue("Admin123!")]
    public required string Password { get; set; }
}