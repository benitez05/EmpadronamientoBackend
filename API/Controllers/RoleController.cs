using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.Domain.Entities;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Infrastructure.Persistence;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AuthLvl("r", 1)]
    [EndpointSummary("Listar roles ")]
    [EndpointDescription("Obtiene todos los roles registrados, incluyendo sus permisos y módulos asociados de forma simplificada.")]
    [ProducesResponseType(typeof(PagedResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var query = _context.Roles
            .Include(r => r.Permisos)
                .ThenInclude(p => p.Modulo)
            .AsQueryable();

        var totalRecords = await query.CountAsync();

        var roles = await query
            .OrderBy(r => r.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Paged(roles.ToResponseList(), pagination, totalRecords, "Roles recuperados exitosamente.");
    }

    [HttpPost]
    [AuthLvl("r", 2)]
    [EndpointSummary("Crear nuevo rol")]
    [EndpointDescription("Registra un nuevo rol en la base de datos. El nombre debe ser único.")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RoleRequest request)
    {
        if (await _context.Roles.AnyAsync(r => r.Nombre == request.Nombre))
        {
            return Error($"El rol '{request.Nombre}' ya existe.");
        }

        var nuevoRol = request.ToEntity();
        _context.Roles.Add(nuevoRol);
        await _context.SaveChangesAsync();

        return Result(nuevoRol.ToResponse(), "Rol creado correctamente.");
    }

    [HttpPost("assign-permission")]
    [AuthLvl("r", 2)]
    [EndpointSummary("Asignar o actualizar permisos")]
    [EndpointDescription("Vincula un rol con un módulo y le asigna un nivel de acceso (1-5). Si la relación ya existe, actualiza el nivel.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPermission([FromBody] AsignarPermisoRequest request)
    {
        // Validación de rango de nivel
        if (request.Lvl < 1 || request.Lvl > 5) return Error("El nivel debe estar entre 1 y 5.");

        if (!await _context.Roles.AnyAsync(r => r.Id == request.RoleId)) return Error("Rol no encontrado.");
        if (!await _context.Modulos.AnyAsync(m => m.Id == request.ModuloId)) return Error("Módulo no encontrado.");

        var permiso = await _context.RolesPermisos
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.ModuloId == request.ModuloId);

        if (permiso == null)
        {
            permiso = new RolePermiso { RoleId = request.RoleId, ModuloId = request.ModuloId, Lvl = request.Lvl };
            _context.RolesPermisos.Add(permiso);
        }
        else
        {
            permiso.Lvl = request.Lvl;
        }

        await _context.SaveChangesAsync();
        return Result(true, "Permiso asignado exitosamente.");
    }

    [HttpDelete("{id}")]
    [AuthLvl("r", 3)]
    [EndpointSummary("Eliminar un rol")]
    [EndpointDescription("Borra un rol de la base de datos siempre y cuando no tenga usuarios vinculados a él.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return Error("Rol no encontrado.");

        // Regla de integridad de negocio
        if (await _context.Usuarios.AnyAsync(u => u.RoleId == id))
        {
            return Error("No se puede eliminar el rol porque tiene usuarios asociados.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return Result(true, "Rol eliminado correctamente.");
    }
}