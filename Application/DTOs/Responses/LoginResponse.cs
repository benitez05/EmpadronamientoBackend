namespace EmpadronamientoBackend.Application.DTOs.Responses;

public class LoginResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required UsuarioResponse Usuario { get; set; }
}