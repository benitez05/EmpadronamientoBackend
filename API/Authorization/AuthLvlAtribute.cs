using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace BenitezLabs.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthLvlAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _k;
    private readonly int _l;

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

        // 2. BYPASS DEFINITIVO: Validar por el "Tipo" (Nivel de Seguridad)
        // Buscamos el claim "tipo" que debe ser '3' para el SuperAdmin
        var tipoClaim = user.FindFirst("tipo")?.Value;
        if (tipoClaim == "3") return; // ¡MODO DIOS! Acceso total sin leer JSON

        // 3. LÓGICA DE PERMISOS PARA MORTALES:
        var permisosJson = user.FindFirst("permisos")?.Value;

        if (!string.IsNullOrEmpty(permisosJson))
        {
            try
            {
                // Deserializamos el diccionario { "u": 2, "r": 1 }
                var permisos = JsonSerializer.Deserialize<Dictionary<string, int>>(permisosJson);

                if (permisos != null && permisos.TryGetValue(_k, out int userLevel))
                {
                    if (userLevel >= _l) return; // ACCESO CONCEDIDO
                }
            }
            catch
            {
                // Error en JSON = Denegado
            }
        }

        // 4. Falló todo: No es SuperAdmin y no tiene el permiso requerido
        context.HttpContext.Items["AuthError_Key"] = _k;
        context.HttpContext.Items["AuthError_Level"] = _l;
        context.Result = new ForbidResult();
    }
}