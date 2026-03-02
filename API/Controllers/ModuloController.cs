using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.Persistence;
using BenitezLabs.Domain.Entities;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Mappers;

namespace EmpadronamientoBackend.API.Controllers;

/// <summary>
/// Controlador para la gestión del catálogo de módulos del sistema.
/// </summary>
[Route("api/[controller]")]
public class ModulosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public ModulosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista de todos los módulos registrados con paginación.
    /// </summary>
    /// <param name="pagination">Parámetros de paginación (PageNumber, PageSize).</param>
    /// <response code="200">Lista de módulos devuelta con éxito.</response>
    [HttpGet]
    [AuthLvl("m", 1)]
    [ProducesResponseType(typeof(PagedResponse<ModuloResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        // 1. IQueryable para construcción de la consulta
        var query = _context.Modulos.AsQueryable();

        // 2. Conteo total de módulos en el sistema
        var totalRecords = await query.CountAsync();

        // 3. Ordenamiento por nombre y aplicación de paginación
        var modulos = await query
            .OrderBy(m => m.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        // 4. Mapeo a DTO y respuesta usando el estándar Paged del BaseController
        return Paged(modulos.ToResponseList(), pagination, totalRecords, "Módulos recuperados exitosamente.");
    }

    /// <summary>
    /// Crea un nuevo módulo en el sistema.
    /// </summary>
    /// <param name="request">Datos del módulo (Nombre y K).</param>
    [HttpPost]
    [AuthLvl("m", 2)]
    [ProducesResponseType(typeof(ApiResponse<ModuloResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] ModuloRequest request)
    {
        if (await _context.Modulos.AnyAsync(m => m.K == request.K))
        {
            return Error($"La clave '{request.K}' ya está en uso por otro módulo.");
        }

        var nuevoModulo = request.ToEntity();

        _context.Modulos.Add(nuevoModulo);
        await _context.SaveChangesAsync();

        return Result(nuevoModulo.ToResponse(), "Módulo creado correctamente.");
    }

    /// <summary>
    /// Actualiza un módulo existente.
    /// </summary>
    /// <param name="id">ID del módulo a editar.</param>
    /// <param name="request">Nuevos datos del módulo.</param>
    [HttpPut("{id}")]
    [AuthLvl("m", 2)]
    [ProducesResponseType(typeof(ApiResponse<ModuloResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] ModuloRequest request)
    {
        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        if (modulo.K != request.K && await _context.Modulos.AnyAsync(m => m.K == request.K))
        {
            return Error($"La clave '{request.K}' ya está siendo usada por otro módulo.");
        }

        modulo.Nombre = request.Nombre;
        modulo.K = request.K;

        await _context.SaveChangesAsync();
        return Result(modulo.ToResponse(), "Módulo actualizado con éxito.");
    }

    /// <summary>
    /// Elimina un módulo del catálogo.
    /// </summary>
    /// <param name="id">ID del módulo a eliminar.</param>
    [HttpDelete("{id}")]
    [AuthLvl("m", 3)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        var tieneDependencias = await _context.RolesPermisos.AnyAsync(rp => rp.ModuloId == id);
        if (tieneDependencias)
        {
            return Error("No se puede eliminar: existen roles con permisos asignados a este módulo.");
        }

        _context.Modulos.Remove(modulo);
        await _context.SaveChangesAsync();

        return Result(true, "Módulo eliminado correctamente.");
    }
}