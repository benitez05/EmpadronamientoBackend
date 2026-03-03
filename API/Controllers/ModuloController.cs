using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Infrastructure.Persistence;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModulosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public ModulosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AuthLvl("m", 1)]
    [EndpointSummary("Listado de módulos")]
    [EndpointDescription("Obtiene todos los módulos del sistema. Útil para cargar menús dinámicos o selectores de permisos.")]
    [ProducesResponseType(typeof(PagedResponse<ModuloResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var query = _context.Modulos.AsQueryable();

        var totalRecords = await query.CountAsync();

        var modulos = await query
            .OrderBy(m => m.Nombre)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Paged(modulos.ToResponseList(), pagination, totalRecords, "Módulos recuperados exitosamente.");
    }

    [HttpPost]
    [AuthLvl("m", 2)]
    [EndpointSummary("Registrar nuevo módulo")]
    [EndpointDescription("Crea una nueva sección en el catálogo. La clave 'K' debe ser única y corta (ej. 'u' para usuarios).")]
    [ProducesResponseType(typeof(ApiResponse<ModuloResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

    [HttpPut("{id}")]
    [AuthLvl("m", 2)]
    [EndpointSummary("Editar módulo")]
    [EndpointDescription("Actualiza el nombre o la clave identificadora de un módulo existente.")]
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

    [HttpDelete("{id}")]
    [AuthLvl("m", 3)]
    [EndpointSummary("Eliminar módulo")]
    [EndpointDescription("Borra un módulo si no tiene permisos asignados a ningún rol. Cuidado: borrar esto puede afectar la navegación del front.")]
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