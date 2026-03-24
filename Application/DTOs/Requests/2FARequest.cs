namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class Login2FARequest
{
    public required string TempToken { get; set; }
    public required string Codigo { get; set; }
}

public class Confirmar2FARequest
{
    public required string Codigo { get; set; }
}