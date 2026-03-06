using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class RefreshTokenRequest
{
    [Required]
    [DefaultValue("admin@benitezlabs.com")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [DefaultValue("8x9b2c3d4e5f6g7h8i9j0k1l2m3n4o5pQWERTY")]
    public string RefreshToken { get; set; } = string.Empty;
}