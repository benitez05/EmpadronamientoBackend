using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BenitezLabs.API.Authorization;

/// <summary>
/// Valida el nivel de acceso por módulo. Permite bypass total al rol 'SuperAdmin'.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthLvlAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _k; // Key del módulo
    private readonly int _l;    // Nivel requerido

    public AuthLvlAttribute(string k, int l)
    {
        _k = k;
        _l = l;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 1. Validar autenticación base
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 2. Bypass para SuperAdmin
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
        if (userRole == "SuperAdmin") return;

        // 3. Validación de permisos granulares por módulo
        var claim = user.FindFirst(_k);
        if (claim == null || !int.TryParse(claim.Value, out int userLevel) || userLevel < _l)
        {
            // Metadatos para el middleware de respuesta de error
            context.HttpContext.Items["AuthError_Key"] = _k;
            context.HttpContext.Items["AuthError_Level"] = _l;
            
            context.Result = new ForbidResult();
        }
    }
}