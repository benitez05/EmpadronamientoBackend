using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Infrastructure.Persistence;
using EmpadronamientoBackend.Application.Interfaces;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModulosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser; // <--- Inyectamos el servicio de usuario

    public ModulosController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AuthLvl("m", 1)]
    [EndpointSummary("Listado completo de módulos (sin paginar)")]
    public async Task<IActionResult> GetAll()
    {
        // 1. Obtenemos todos los módulos de la base de datos sin Skip ni Take
        var modulos = await _context.Modulos
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        // 2. Usamos el método Result heredado de BaseController
        //    Esto devolverá un ApiResponse estándar en lugar de un PagedResponse
        return Result(modulos.ToResponseList(), "Módulos recuperados exitosamente.");
    }

    [HttpPost]
    [AuthLvl("m", 2)]
    [EndpointSummary("Registrar nuevo módulo")]
    public async Task<IActionResult> Create([FromBody] ModuloRequest request)
    {
        // --- VALIDACIÓN DE NIVEL DE STAFF ---
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para crear módulos del sistema. Solo personal autorizado.");

        if (await _context.Modulos.AnyAsync(m => m.K == request.K))
            return Error($"La clave '{request.K}' ya está en uso.");

        var nuevoModulo = request.ToEntity();
        _context.Modulos.Add(nuevoModulo);
        await _context.SaveChangesAsync();

        return Result(nuevoModulo.ToResponse(), "Módulo creado correctamente.");
    }

    [HttpPut("{id}")]
    [AuthLvl("m", 2)]
    [EndpointSummary("Editar módulo")]
    public async Task<IActionResult> Update(int id, [FromBody] ModuloRequest request)
    {
        // --- VALIDACIÓN DE NIVEL DE STAFF ---
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para modificar módulos del sistema.");

        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        if (modulo.K != request.K && await _context.Modulos.AnyAsync(m => m.K == request.K))
            return Error($"La clave '{request.K}' ya está siendo usada.");

        modulo.Nombre = request.Nombre;
        modulo.K = request.K;

        await _context.SaveChangesAsync();
        return Result(modulo.ToResponse(), "Módulo actualizado con éxito.");
    }

    [HttpDelete("{id}")]
    [AuthLvl("m", 3)]
    [EndpointSummary("Eliminar módulo")]
    public async Task<IActionResult> Delete(int id)
    {
        // --- VALIDACIÓN DE NIVEL DE STAFF ---
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para eliminar módulos del sistema.");

        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        var tieneDependencias = await _context.RolesPermisos.AnyAsync(rp => rp.ModuloId == id);
        if (tieneDependencias)
            return Error("No se puede eliminar: existen roles con permisos asignados.");

        _context.Modulos.Remove(modulo);
        await _context.SaveChangesAsync();

        return Result(true, "Módulo eliminado correctamente.");
    }
}