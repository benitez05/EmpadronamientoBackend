using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EmpadronamientoBackend.Application.DTOs.Responses;

namespace EmpadronamientoBackend.API.Filters;

/// <summary>
/// Filtro global que transforma errores de ModelState
/// en ApiResponse uniforme.
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errors = context.ModelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        context.Result = new BadRequestObjectResult(
            ApiResponseFactory.Fail<object>(
                "Validation error",
                errors));
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}