using EmpadronamientoBackend.Application.Interfaces;
using Resend;

namespace EmpadronamientoBackend.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;

    public ResendEmailService(IResend resend)
    {
        _resend = resend;
    }

    public async Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string contenidoHtml)
    {
        try
        {
            var message = new EmailMessage
            {
                // ¡AQUÍ ESTÁ LA MAGIA! Ya usamos tu subdominio real
                From = "Sistema Empadronamiento <no-reply@notificaciones.spii.mx>",
                To = { destinatario },
                Subject = asunto,
                HtmlBody = contenidoHtml
            };

            var response = await _resend.EmailSendAsync(message);
            return response != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}