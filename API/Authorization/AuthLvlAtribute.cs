using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json; // <--- AGREGAR ESTO PARA EL JSON

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

        // 2. Bypass para SuperAdmin
        var userRole = user.FindFirst("role")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "SuperAdmin") return;

        // 3. NUEVA LÓGICA: Buscar el paquete de "permisos"
        var permisosJson = user.FindFirst("permisos")?.Value;
        
        if (!string.IsNullOrEmpty(permisosJson))
        {
            try
            {
                // Deserializamos el diccionario plano que creamos en el PasswordService
                var permisos = JsonSerializer.Deserialize<Dictionary<string, int>>(permisosJson);

                // Verificamos si existe la llave del módulo (ej: "u") y si el nivel es suficiente
                if (permisos != null && permisos.TryGetValue(_k, out int userLevel) && userLevel >= _l)
                {
                    return; // ¡ACCESO CONCEDIDO!
                }
            }
            catch
            {
                // Si el JSON viene mal, denegamos por seguridad
            }
        }

        // 4. Si llegó aquí, no tiene permiso o no se encontró el módulo
        context.HttpContext.Items["AuthError_Key"] = _k;
        context.HttpContext.Items["AuthError_Level"] = _l;
        context.Result = new ForbidResult();
    }
}