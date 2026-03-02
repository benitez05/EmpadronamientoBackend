using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BenitezLabs.API.Authorization;

/// <summary>
/// Valida si el usuario posee uno de los roles permitidos en el sistema.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HasRoleAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public HasRoleAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 1. Verificar si el usuario está autenticado
        var user = context.HttpContext.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 2. Extraer el rol del Claim estándar de .NET
        var userRole = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        // 3. Validar si el rol del usuario está en la lista de permitidos
        if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
        {
            context.Result = new ForbidResult();
        }
    }
}