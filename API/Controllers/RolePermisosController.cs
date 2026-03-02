using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.Persistence;
using BenitezLabs.Domain.Entities;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.DTOs.Requests;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controlador para la gestión granular de la matriz de permisos.
/// </summary>
[Route("api/[controller]")]
public class RolePermisosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RolePermisosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la matriz completa de permisos del sistema con paginación.
    /// </summary>
    /// <param name="pagination">Parámetros de paginación (PageNumber, PageSize).</param>
    [HttpGet("matrix")]
    [AuthLvl("r", 1)]
    [ProducesResponseType(typeof(PagedResponse<PermisoMatrixResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionMatrix([FromQuery] PaginationParams pagination)
    {
        // 1. IQueryable para construcción de la consulta con sus relaciones
        var query = _context.RolesPermisos
            .Include(rp => rp.Role)
            .Include(rp => rp.Modulo)
            .AsQueryable();

        // 2. Conteo total antes de paginar
        var totalRecords = await query.CountAsync();

        // 3. Aplicación de paginación
        // Ordenamos por Rol y luego por Módulo para que la matriz sea legible
        var matrix = await query
            .OrderBy(rp => rp.Role.Nombre)
            .ThenBy(rp => rp.Modulo.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        // 4. Mapeo y respuesta estandarizada
        return Paged(matrix.ToMatrixList(), pagination, totalRecords, "Matriz de permisos recuperada.");
    }

    /// <summary>
    /// Elimina un permiso específico de un rol (Revoca el acceso).
    /// </summary>
    [HttpDelete]
    [AuthLvl("r", 3)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokePermission([FromQuery] int roleId, [FromQuery] int moduloId)
    {
        var permiso = await _context.RolesPermisos
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.ModuloId == moduloId);

        if (permiso == null) return Error("El permiso no existe.");

        _context.RolesPermisos.Remove(permiso);
        await _context.SaveChangesAsync();

        return Result(true, "Permiso revocado exitosamente.");
    }
}