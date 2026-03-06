using System.ComponentModel;

namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class UpdateUsuarioRequest
{
    [DefaultValue("Roberto")]
    public string Nombre { get; set; } = null!;

    [DefaultValue("Benitez")]
    public string Apellidos { get; set; } = null!;

    [DefaultValue("8112345678")]
    public string? Celular { get; set; }

    [DefaultValue("https://benitezlabs.com/avatar.png")]
    public string? Imagen { get; set; }

    // El RoleId es opcional en el request por si solo quieren editar el perfil
    [DefaultValue(1)]
    public int? RoleId { get; set; }
}