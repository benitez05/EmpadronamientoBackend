namespace EmpadronamientoBackend.Application.DTOs.Responses;

/// <summary>
/// Wrapper estándar para todas las respuestas del API.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje informativo.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos retornados.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Código interno opcional.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Timestamp UTC de respuesta.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}