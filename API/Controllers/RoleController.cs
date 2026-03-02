using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.Persistence;
using BenitezLabs.Domain.Entities;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controlador para la gestión de roles y asignación de permisos modulares.
/// </summary>
[Route("api/[controller]")]
public class RolesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista de roles con sus permisos simplificados y paginación.
    /// </summary>
    /// <param name="pagination">Parámetros de paginación (PageNumber, PageSize).</param>
    [HttpGet]
    [AuthLvl("r", 1)]
    [ProducesResponseType(typeof(PagedResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        // 1. IQueryable con carga profunda de permisos y módulos
        var query = _context.Roles
            .Include(r => r.Permisos)
                .ThenInclude(p => p.Modulo)
            .AsQueryable();

        // 2. Conteo total de roles registrados
        var totalRecords = await query.CountAsync();

        // 3. Ordenamiento alfabético y paginación
        var roles = await query
            .OrderBy(r => r.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        // 4. Mapeo a DTO de respuesta usando tu método Paged del BaseController
        return Paged(roles.ToResponseList(), pagination, totalRecords, "Roles recuperados exitosamente.");
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    [HttpPost]
    [AuthLvl("r", 2)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Asigna o actualiza un permiso específico para un rol y módulo.
    /// </summary>
    /// <remarks>
    /// El nivel debe estar entre 1 y 5.
    /// </remarks>
    [HttpPost("assign-permission")]
    [AuthLvl("r", 2)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPermission([FromBody] AsignarPermisoRequest request)
    {
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

    /// <summary>
    /// Elimina un rol si no tiene usuarios asociados.
    /// </summary>
    [HttpDelete("{id}")]
    [AuthLvl("r", 3)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return Error("Rol no encontrado.");

        if (await _context.Usuarios.AnyAsync(u => u.RoleId == id))
        {
            return Error("No se puede eliminar el rol porque tiene usuarios asociados.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return Result(true, "Rol eliminado correctamente.");
    }
}