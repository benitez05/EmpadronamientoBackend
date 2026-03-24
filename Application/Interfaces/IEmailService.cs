namespace EmpadronamientoBackend.Application.Interfaces;

public interface IEmailService
{
    
    Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string contenidoHtml);
}