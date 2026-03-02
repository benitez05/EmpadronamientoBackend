using EmpadronamientoBackend.API.Middleware;
using Scalar.AspNetCore; // <-- Obligatorio para la nueva interfaz visual

namespace EmpadronamientoBackend.API.Extensions;

/// <summary>
/// Configuración del pipeline HTTP.
/// </summary>
public static class AppExtensions
{
    public static WebApplication UseApiTemplate(
        this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseMiddleware<ExceptionMiddleware>();

        // OpenAPI y Scalar (Reemplaza a Swagger UI)
        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseAuthentication(); 
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}