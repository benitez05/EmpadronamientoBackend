namespace EmpadronamientoBackend.Application.DTOs.Requests;
public class CrearRedSocialRequest
{
    // 🔥 Se agrega el Id opcional para actualización
    public int? Id { get; set; }

    public required string TipoRedSocial { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? UrlPerfil { get; set; }
}