namespace EmpadronamientoBackend.Application.DTOs.Responses;

/// <summary>
/// Factory centralizada para crear respuestas estándar.
/// </summary>
public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(
        T data,
        string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail<T>(
        string message,
        List<string>? errors = null,
        string? code = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Code = code
        };
    }
}