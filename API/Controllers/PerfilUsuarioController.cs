using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controlador para la autogestión del perfil y sesiones del usuario autenticado.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PerfilUsuarioController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;
    private readonly IS3Service _s3Service;

    public PerfilUsuarioController(
        ApplicationDbContext context,
        ICacheService cacheService,
        IS3Service s3Service,
        ICurrentUserService currentUser)
    {
        _context = context;
        _cacheService = cacheService;
        _currentUser = currentUser;
        _s3Service = s3Service;
    }

    /// <summary>
    /// Obtiene la información del perfil del usuario autenticado.
    /// </summary>
    /// <returns>Los datos detallados del usuario logueado.</returns>
    [HttpGet]
    [AuthLvl("u", 1)]
    [EndpointSummary("Obtiene tu propia información de usuario")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(_currentUser.UserId!);
        
        var usuario = await _context.Usuarios
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario == null) return Error("Usuario no encontrado.");

        return Result(usuario.ToResponse(_s3Service), "Detalle de tu perfil recuperado.");
    }

    /// <summary>
    /// Actualiza la información del perfil del usuario autenticado.
    /// </summary>
    /// <param name="request">Datos a actualizar, incluyendo la imagen opcional.</param>
    /// <returns>El perfil actualizado.</returns>
    [HttpPut]
    [AuthLvl("u", 1)]
    [EndpointSummary("Editar tu propio perfil de usuario")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateUsuarioRequest request)
    {
        var userId = int.Parse(_currentUser.UserId!);
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (usuario == null)
        {
            return Error("Usuario no encontrado.");
        }

        string previousImage = usuario.Imagen;

        // Subida de imagen si se proporciona
        if (request.Imagen != null && request.Imagen.Length > 0)
        {
            try
            {
                var pathSugerido = $"usuarios/{usuario.OrganizacionId}/{Guid.NewGuid()}{Path.GetExtension(request.Imagen.FileName)}";
                using var stream = request.Imagen.OpenReadStream();

                var keyFinal = await _s3Service.UploadImageAsync(stream, pathSugerido, request.Imagen.ContentType);

                usuario.Imagen = keyFinal;

                if (!string.IsNullOrEmpty(previousImage))
                {
                    try
                    {
                        await _s3Service.DeleteImageAsync(previousImage);
                    }
                    catch (Exception)
                    {
                        // Se ignora para no interrumpir la actualización
                    }
                }
            }
            catch (Exception)
            {
                return Error("No se pudo subir la imagen proporcionada.");
            }
        }

        usuario.Nombre = request.Nombre;
        usuario.Apellidos = request.Apellidos;
        usuario.Celular = request.Celular;

        await _context.SaveChangesAsync();

        return Result(usuario.ToResponse(_s3Service), "Tus datos han sido actualizados.");
    }

    /// <summary>
    /// Lista las sesiones activas del usuario autenticado.
    /// </summary>
    /// <returns>Lista de dispositivos vinculados.</returns>
    [HttpGet("sessions")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Listar tus sesiones activas")]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = int.Parse(_currentUser.UserId!);
        var currentJti = _currentUser.Jti;

        var sesiones = await _context.UsuarioSesiones
            .Where(s => s.UsuarioId == userId)
            .Select(s => new
            {
                s.Id,
                s.DeviceInfo,
                s.IpAddress,
                s.FechaCreacion,
                EsActual = s.Jti == currentJti
            })
            .ToListAsync();

        return Result(sesiones, "Lista de dispositivos vinculados.");
    }

    /// <summary>
    /// Cierra una sesión remota específica del usuario autenticado.
    /// </summary>
    /// <param name="id">El ID de la sesión a revocar.</param>
    /// <returns>Confirmación de desconexión.</returns>
    [HttpDelete("sessions/{id}")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Cerrar tu sesión remota")]
    public async Task<IActionResult> RevokeSession(int id)
    {
        var userId = int.Parse(_currentUser.UserId!);

        var sesion = await _context.UsuarioSesiones
            .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == userId);

        if (sesion == null) return Error("Sesión no encontrada.");

        await _cacheService.SetAsync($"revoked_{sesion.Jti}", true, TimeSpan.FromHours(2));

        _context.UsuarioSesiones.Remove(sesion);
        await _context.SaveChangesAsync();

        return Result(true, "Dispositivo desconectado correctamente.");
    }
}