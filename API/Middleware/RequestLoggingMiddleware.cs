namespace EmpadronamientoBackend.API.Middleware;

/// <summary>
/// Middleware para logging automático de cada request HTTP.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation(
            "HTTP {Method} {Path} executed at {Time}",
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow);

        await _next(context);
    }
}