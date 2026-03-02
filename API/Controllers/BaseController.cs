using Microsoft.AspNetCore.Mvc;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controller base del sistema.
/// Centraliza respuestas HTTP usando ApiResponse.
/// Evita repetir Ok(), BadRequest(), etc.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Respuesta exitosa estándar.
    /// </summary>
    protected IActionResult Result<T>(T data, string message = "Success")
    {
        return Ok(ApiResponseFactory.Success(data, message));
    }

    /// <summary>
    /// Respuesta de error estándar.
    /// </summary>
    protected IActionResult Error(string message, params string[] errors)
    {
        return BadRequest(
            ApiResponseFactory.Fail<object>(
                message,
                errors.ToList()));
    }

    /// <summary>
    /// Respuesta paginada estándar.
    /// </summary>
    protected IActionResult Paged<T>(
        IEnumerable<T> data,
        PaginationParams pagination,
        int totalRecords,
        string message = "Success")
    {
        var response = new PagedResponse<T>(
            data,
            pagination.PageNumber,
            pagination.PageSize,
            totalRecords,
            message);

        return Ok(response);
    }
}