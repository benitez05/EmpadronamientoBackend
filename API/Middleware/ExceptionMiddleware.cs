using System.Net;
using System.Text.Json;
using EmpadronamientoBackend.Application.DTOs.Responses;

namespace EmpadronamientoBackend.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado detectado en BenitezLabs API");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Si estamos en Desarrollo, mostramos el error real. 
        // En Producción, mandamos un mensaje genérico por seguridad.
        var message = _env.IsDevelopment() 
            ? exception.Message 
            : "Ocurrió un error interno en el servidor. Intente más tarde.";

        var errors = _env.IsDevelopment() 
            ? new List<string> { exception.StackTrace?.ToString() ?? "" } 
            : new List<string> { "Contacte al administrador del sistema." };

        var response = ApiResponseFactory.Fail<object>(
            message, 
            errors, 
            "INTERNAL_SERVER_ERROR"
        );

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}