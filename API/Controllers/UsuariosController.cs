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

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser; // <--- Nuestra nueva estrella

    public UsuariosController(
        ApplicationDbContext context, 
        ICacheService cacheService, 
        ICurrentUserService currentUser)
    {
        _context = context;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    #region GESTIÓN DE SESIONES Y SEGURIDAD (USER ME)

    [HttpGet("me/sessions")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Listar sesiones activas del usuario logueado")]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = int.Parse(_currentUser.UserId!);
        var currentJti = _currentUser.Jti;

        var sesiones = await _context.UsuarioSesiones
            .Where(s => s.UsuarioId == userId)
            .Select(s => new {
                s.Id,
                s.DeviceInfo,
                s.IpAddress,
                s.FechaCreacion,
                EsActual = s.Jti == currentJti // Comparamos con el JTI del Token actual
            })
            .ToListAsync();

        return Result(sesiones, "Lista de dispositivos vinculados.");
    }

    [HttpDelete("me/sessions/{id}")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Cerrar sesión remota propia")]
    public async Task<IActionResult> RevokeSession(int id)
    {
        var userId = int.Parse(_currentUser.UserId!);

        var sesion = await _context.UsuarioSesiones
            .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == userId);

        if (sesion == null) return Error("Sesión no encontrada.");

        // Invalidar el JTI en el Caché de inmediato usando el JTI de la sesión encontrada
        await _cacheService.SetAsync($"revoked_{sesion.Jti}", true, TimeSpan.FromHours(2));

        _context.UsuarioSesiones.Remove(sesion);
        await _context.SaveChangesAsync();

        return Result(true, "Dispositivo desconectado correctamente.");
    }

    #endregion

    #region ADMINISTRACIÓN DE USUARIOS (CRUD & STATUS)

    [HttpGet]
    [AuthLvl("u", 4)]
    [EndpointSummary("Listado de usuarios")]
    public async Task<IActionResult> GetAll([FromQuery] UsuarioFilterParams filter, [FromQuery] PaginationParams pagination)
    {
        var query = _context.Usuarios.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Busqueda))
        {
            var b = filter.Busqueda.ToLower();
            query = query.Where(u => u.Nombre.ToLower().Contains(b) 
                                  || u.Apellidos.ToLower().Contains(b) 
                                  || u.Correo.ToLower().Contains(b));
        }

        var totalRecords = await query.CountAsync();

        var usuarios = await query
            .OrderByDescending(u => u.FechaCreacion)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Paged(usuarios.ToResponseList(), pagination, totalRecords, "Usuarios recuperados.");
    }

    [HttpGet("{id}")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Detalle del usuario")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) return Error("Usuario no encontrado.");

        return Result(usuario.ToResponse(), "Detalle del usuario recuperado.");
    }

    [HttpPut("{id}/role")]
    [AuthLvl("u", 3)]
    [EndpointSummary("Actualizar rol de usuario")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] int newRoleId)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return Error("Usuario no encontrado.");

        if (!await _context.Roles.AnyAsync(r => r.Id == newRoleId))
            return Error("El rol especificado no existe.");

        usuario.RoleId = newRoleId;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Result(true, "Rol actualizado correctamente.");
    }

    [HttpPatch("{id}/status")]
    [AuthLvl("u", 2)]
    [EndpointSummary("Baneo / Activación de cuenta")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return Error("Usuario no encontrado.");

        usuario.Activo = !usuario.Activo;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        string estado = usuario.Activo ? "activado" : "desactivado";
        return Result(usuario.Activo, $"El usuario ha sido {estado} correctamente.");
    }

    #endregion

    #region REPORTES Y ESTADÍSTICAS

    [HttpGet("stats")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Resumen de estadísticas")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _context.Usuarios
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Activos = g.Count(u => u.Activo),
                Inactivos = g.Count(u => !u.Activo)
            })
            .FirstOrDefaultAsync();

        var data = stats ?? new { Total = 0, Activos = 0, Inactivos = 0 };

        return Result(data, "Estadísticas recuperadas exitosamente.");
    }

    #endregion
}