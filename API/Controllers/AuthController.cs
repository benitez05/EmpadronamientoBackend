using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Infrastructure.Identity;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BenitezLabs.Persistence;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controlador encargado de la autenticación y gestión de usuarios.
/// </summary>
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;

    public AuthController(ApplicationDbContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Registra un nuevo usuario en la plataforma.
    /// </summary>
    /// <param name="request">Datos del registro.</param>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
        {
            return Error("Ya existe una cuenta asociada a este correo electrónico.");
        }

        var user = request.ToEntity();
        user.PasswordHash = _passwordService.HashPassword(null!, request.Password);
        user.RoleId = 2; // Rol por defecto: Usuario

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        return Result(user.ToResponse(), "Tu cuenta ha sido creada exitosamente.");
    }

    /// <summary>
    /// Inicia sesión y obtiene tokens de acceso.
    /// </summary>
    /// <param name="request">Credenciales de acceso.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Usuarios
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Correo == request.Correo);

        if (user == null || _passwordService.VerifyPassword(user, request.Password) == PasswordVerificationResult.Failed)
        {
            return Error("El correo o la contraseña son incorrectos.");
        }

        var token = _passwordService.GenerateJwtToken(user);
        var refreshToken = _passwordService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(30);
        user.UltimoLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Usuario = user.ToResponse()
        };

        return Result(response, "Ha iniciado sesión correctamente.");
    }

    /// <summary>
    /// Renueva el Token JWT usando un Refresh Token válido.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = await _context.Usuarios.SingleOrDefaultAsync(u => u.Correo == request.Correo);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiration < DateTime.UtcNow)
        {
            return Error("El token de refresco es inválido o ha expirado.");
        }

        var token = _passwordService.GenerateJwtToken(user);
        var newRefreshToken = _passwordService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(30);

        await _context.SaveChangesAsync();

        return Result(new { Token = token, RefreshToken = newRefreshToken }, "Token renovado exitosamente.");
    }

    /// <summary>
    /// Actualiza la contraseña del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpPut("update-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return Error("No se pudo identificar al usuario.");

        var user = await _context.Usuarios.FindAsync(userId);
        if (user == null) return Error("Usuario no encontrado.");

        user.PasswordHash = _passwordService.HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();

        return Result((object)null!, "Tu contraseña ha sido actualizada con éxito.");
    }
}