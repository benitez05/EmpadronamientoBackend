using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Infrastructure.Persistence;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController] // Importante para que valide automáticamente los modelos
public class RolePermisosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RolePermisosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("matrix")]
    [AuthLvl("r", 1)]
    [EndpointSummary("Consultar matriz de permisos")]
    [EndpointDescription("Obtiene la relación completa entre Roles y Módulos, detallando qué nivel de acceso tiene cada uno.")]
    [ProducesResponseType(typeof(PagedResponse<PermisoMatrixResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionMatrix([FromQuery] PaginationParams pagination)
    {
        var query = _context.RolesPermisos
            .Include(rp => rp.Role)
            .Include(rp => rp.Modulo)
            .AsQueryable();

        var totalRecords = await query.CountAsync();

        // Ordenamos para que en la UI del Front se vea organizado por grupos
        var matrix = await query
            .OrderBy(rp => rp.Role.Nombre)
            .ThenBy(rp => rp.Modulo.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Paged(matrix.ToMatrixList(), pagination, totalRecords, "Matriz de permisos recuperada.");
    }

    [HttpDelete]
    [AuthLvl("r", 3)]
    [EndpointSummary("Revocar acceso a módulo")]
    [EndpointDescription("Elimina la relación entre un Rol y un Módulo. El rol perderá cualquier tipo de acceso al módulo especificado.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokePermission([FromQuery] int roleId, [FromQuery] int moduloId)
    {
        var permiso = await _context.RolesPermisos
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.ModuloId == moduloId);

        if (permiso == null) return Error("El permiso no existe o ya fue revocado.");

        _context.RolesPermisos.Remove(permiso);
        await _context.SaveChangesAsync();

        return Result(true, "Permiso revocado exitosamente.");
    }
}