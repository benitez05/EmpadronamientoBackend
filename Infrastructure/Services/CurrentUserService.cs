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

    public int Tipo 
    {
        get
        {
            var tipoClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tipo");
            return int.TryParse(tipoClaim, out var t) ? t : 0;
        }
    }

    public int OrganizacionId 
    {
        get
        {
            var orgClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("OrganizacionId");
            // Si no hay claim, devolvemos 0 para que no truene la interfaz
            return int.TryParse(orgClaim, out var id) ? id : 0;
        }
    }

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? Dispositivo => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}