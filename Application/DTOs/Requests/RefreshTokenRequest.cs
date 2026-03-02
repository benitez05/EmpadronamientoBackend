using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class RefreshTokenRequest
{
    /// <summary>Correo asociado al token</summary>
    [DefaultValue("admin@benitezlabs.com")]
    public required string Correo { get; set; }

    /// <summary>Token de refresco obtenido en el Login</summary>
    [DefaultValue("8x9b2c3d4e5f6g7h8i9j0k1l2m3n4o5pQWERTY")]
    public required string RefreshToken { get; set; }
}