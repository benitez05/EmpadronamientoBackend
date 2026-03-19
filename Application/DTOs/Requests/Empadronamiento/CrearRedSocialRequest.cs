namespace EmpadronamientoBackend.Application.DTOs.Requests;
public class CrearRedSocialRequest
{
    public required string TipoRedSocial { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string? UrlPerfil { get; set; }
}