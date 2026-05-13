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
/// Controlador para la administración de usuarios del sistema.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsuariosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IS3Service _s3Service;

    public UsuariosController(
        ApplicationDbContext context,
        IS3Service s3Service,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
        _s3Service = s3Service;
    }

    #region ADMINISTRACIÓN DE USUARIOS (CRUD & STATUS)

    /// <summary>
    /// Obtiene un listado paginado de usuarios.
    /// </summary>
    [HttpGet]
    [AuthLvl("u", 2)]
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

        return Paged(usuarios.ToResponseList(_s3Service), pagination, totalRecords, "Usuarios recuperados.");
    }

    /// <summary>
    /// Obtiene los detalles de un usuario específico.
    /// </summary>
    [HttpGet("{id}")]
    [AuthLvl("u", 1)]
    [EndpointSummary("Obtiene la informacion de un usuario")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) return Error("Usuario no encontrado.");

        return Result(usuario.ToResponse(_s3Service), "Detalle del usuario recuperado.");
    }

    /// <summary>
    /// Edita la información básica y de perfil de un usuario específico.
    /// </summary>
    /// <param name="id">El identificador único del usuario a editar.</param>
    /// <param name="request">Los datos a actualizar, incluyendo la imagen de perfil opcional.</param>
    [HttpPut("{id}")]
    [AuthLvl("u", 2)]
    [EndpointSummary("Editar perfil de un usuario específico")]
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

        return Result(usuario.ToResponse(_s3Service), "Los datos del usuario han sido actualizados.");
    }

    /// <summary>
    /// Modifica el rol de un usuario existente.
    /// </summary>
    [HttpPut("{id}/role")]
    [AuthLvl("u", 2)]
    [EndpointSummary("Actualiza el role de un usuario")]
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

    /// <summary>
    /// Activa o desactiva la cuenta de un usuario.
    /// </summary>
    [HttpPatch("{id}/status")]
    [AuthLvl("u", 2)]
    [EndpointSummary("Alterna el estatus de un usuario entre activado y desactivado")]
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