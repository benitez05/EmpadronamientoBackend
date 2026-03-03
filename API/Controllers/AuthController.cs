using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using EmpadronamientoBackend.Infrastructure.Persistence;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        ApplicationDbContext context, 
        IPasswordService passwordService, 
        ICacheService cacheService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _passwordService = passwordService;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    #region REGISTRO Y LOGIN

    [HttpPost("register")]
    [EndpointSummary("Registro de nuevos usuarios")]
    [EndpointDescription("Crea una cuenta nueva. Por defecto asigna el Rol de 'Usuario' (ID: 2).")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
        {
            return Error("Ya existe una cuenta asociada a este correo electrónico.");
        }

        var user = request.ToEntity();
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);
        user.RoleId = 2;

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        return Result(user.ToResponse(), "Tu cuenta ha sido creada exitosamente.");
    }

    [HttpPost("login")]
    [EndpointSummary("Inicio de sesión")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Usuarios
            .Include(u => u.Role)
                .ThenInclude(r => r.Permisos)
                    .ThenInclude(p => p.Modulo)
            .SingleOrDefaultAsync(u => u.Correo == request.Correo);

        if (user == null || !_passwordService.IsValidPassword(user, request.Password))
        {
            return Error("El correo o la contraseña son incorrectos.");
        }

        var tokenData = _passwordService.GenerateJwtToken(user);
        var refreshToken = _passwordService.GenerateRefreshToken();

        var nuevaSesion = new UsuarioSesion
        {
            UsuarioId = user.Id,
            Jti = tokenData.Jti,
            RefreshToken = refreshToken,
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            FechaExpiracion = DateTime.UtcNow.AddDays(30)
        };

        _context.UsuarioSesiones.Add(nuevaSesion);
        user.UltimoLogin = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return Result(new LoginResponse
        {
            Token = tokenData.Token,
            RefreshToken = refreshToken,
            Usuario = user.ToResponse()
        }, "Ha iniciado sesión correctamente.");
    }

    #endregion

    #region GESTIÓN DE TOKENS (REFRESH & LOGOUT)

    [HttpPost("refresh-token")]
    [EndpointSummary("Renovar Access Token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var sesion = await _context.UsuarioSesiones
            .Include(s => s.Usuario)
                .ThenInclude(u => u.Role)
                    .ThenInclude(r => r.Permisos)
                        .ThenInclude(p => p.Modulo)
            .SingleOrDefaultAsync(s => s.RefreshToken == request.RefreshToken);

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
        {
            return Error("El token de refresco es inválido o ha expirado.");
        }

        var tokenData = _passwordService.GenerateJwtToken(sesion.Usuario);
        var newRefreshToken = _passwordService.GenerateRefreshToken();

        sesion.RefreshToken = newRefreshToken;
        sesion.Jti = tokenData.Jti;
        sesion.FechaExpiracion = DateTime.UtcNow.AddDays(30);

        await _context.SaveChangesAsync();

        return Result(new { Token = tokenData.Token, RefreshToken = newRefreshToken }, "Token renovado exitosamente.");
    }

    [Authorize]
    [HttpPost("logout")]
    [EndpointSummary("Cerrar sesión actual")]
    public async Task<IActionResult> Logout()
    {
        var jti = _currentUser.Jti; // Uso directo del servicio
        
        if (string.IsNullOrEmpty(jti)) return Error("Sesión no identificada.");

        await _cacheService.SetAsync($"revoked_{jti}", true, TimeSpan.FromHours(1));

        var sesion = await _context.UsuarioSesiones.FirstOrDefaultAsync(s => s.Jti == jti);
        if (sesion != null)
        {
            _context.UsuarioSesiones.Remove(sesion);
            await _context.SaveChangesAsync();
        }

        return Result(true, "Has cerrado sesión correctamente.");
    }

    #endregion

    #region SEGURIDAD DE CUENTA

    [Authorize]
    [HttpPut("update-password")]
    [EndpointSummary("Cambiar contraseña")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userId = _currentUser.UserId; // Uso directo del servicio
        if (string.IsNullOrEmpty(userId)) return Error("Usuario no identificado.");

        var user = await _context.Usuarios.FindAsync(int.Parse(userId));
        if (user == null) return Error("Usuario no encontrado.");

        user.PasswordHash = _passwordService.HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();

        return Result(true, "Tu contraseña ha sido actualizada con éxito.");
    }

    #endregion
}