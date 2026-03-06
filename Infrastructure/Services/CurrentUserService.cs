using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using EmpadronamientoBackend.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EmpadronamientoBackend.Infrastructure.Services; 

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                             ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Jti => _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Jti);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // --- EL CAMBIO CRUCIAL PARA EL FILTRO GLOBAL ---

    public int Tipo 
    {
        get
        {
            // Buscamos el claim "tipo" que metimos en el PasswordService
            var tipoClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tipo");
            return int.TryParse(tipoClaim, out var t) ? t : 0; // 0 si no hay token (ej. al migrar)
        }
    }

    public int? OrganizacionId 
    {
        get
        {
            var orgClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("OrganizacionId");
            return int.TryParse(orgClaim, out var id) ? id : null;
        }
    }

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? Dispositivo => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}