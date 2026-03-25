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
    private readonly ICurrentUserService _currentUser;
    private readonly IS3Service _s3Service;

    public UsuariosController(
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

    [HttpPut("{id}")]
    [AuthLvl("u", 2)]
    [EndpointSummary("Editar perfil de usuario")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
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
                // 1. Definimos la subcarpeta funcional
                // Ahora: usuarios / {orgId} / {archivo}
                var pathSugerido = $"usuarios/{usuario.OrganizacionId}/{Guid.NewGuid()}{Path.GetExtension(request.Imagen.FileName)}";
                using var stream = request.Imagen.OpenReadStream();

                // 2. El servicio inyecta el prefijo (dev/prod) y nos da la ruta completa
                var keyFinal = await _s3Service.UploadImageAsync(stream, pathSugerido, request.Imagen.ContentType);

                // 3. Guardamos la key final con prefijo en la BD
                usuario.Imagen = keyFinal;

                // Eliminar imagen anterior si existía
                if (!string.IsNullOrEmpty(previousImage))
                {
                    try
                    {
                        // Como previousImage ya tiene el prefijo, el servicio la encontrará sin problemas
                        await _s3Service.DeleteImageAsync(previousImage);
                    }
                    catch (Exception)
                    {
                        // No interrumpimos la actualización por un fallo en eliminar la imagen anterior
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

        // El Mapper ToResponse usará s3Service.GetFileUrl internamente
        return Result(usuario.ToResponse(_s3Service), "Los datos del usuario han sido actualizados.");
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

        // Enviamos s3Service para que las URLs de las fotos de la lista se generen bien
        return Paged(usuarios.ToResponseList(_s3Service), pagination, totalRecords, "Usuarios recuperados.");
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

        return Result(usuario.ToResponse(_s3Service), "Detalle del usuario recuperado.");
    }

    [HttpPut("{id}/role")]
    [AuthLvl("u", 3)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return Error("Usuario no encontrado o no pertenece a tu organización.");

        var rolValido = await _context.Roles.AnyAsync(r => r.Id == request.NewRoleId);
        if (!rolValido)
            return Error("El rol especificado no existe o no pertenece a la organización del usuario.");

        usuario.RoleId = request.NewRoleId;
        await _context.SaveChangesAsync();

        return Result(true, $"El usuario {usuario.Nombre} ahora tiene un nuevo rol.");
    }

    public record UpdateRoleRequest(int NewRoleId);

    [HttpPatch("{id}/status")]
    [AuthLvl("u", 3)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (id == int.Parse(_currentUser.UserId!))
        {
            return Error("No puedes desactivar tu propia cuenta.");
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return Error("Usuario no encontrado.");

        usuario.Activo = !usuario.Activo;
        await _context.SaveChangesAsync();

        string accion = usuario.Activo ? "activado" : "desactivado";
        return Result(usuario.Activo, $"El usuario {usuario.Nombre} ha sido {accion} correctamente.");
    }

    #endregion
}