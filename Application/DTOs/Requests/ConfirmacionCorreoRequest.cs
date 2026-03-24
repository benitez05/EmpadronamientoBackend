namespace EmpadronamientoBackend.Application.DTOs.Requests;

public class EnviarCodigoRequest
{
    public required string Correo { get; set; }
}

public class VerificarCodigoRequest
{
    public required string Correo { get; set; }
    public required string Codigo { get; set; }
}