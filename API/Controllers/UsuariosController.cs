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
/// Controlador para la administración central de usuarios y sus estados.
/// </summary>
[Route("api/[controller]")]
public class UsuariosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

   /// <summary>
    /// Obtiene la lista de usuarios con filtros y paginación.
    /// </summary>
    /// <param name="filter">Filtros de búsqueda, rol y estado.</param>
    /// <param name="pagination">Parámetros de paginación (PageNumber, PageSize).</param>
    [HttpGet]
    [AuthLvl("u", 1)]
    public async Task<IActionResult> GetAll(
        [FromQuery] UsuarioFilterParams filter, 
        [FromQuery] PaginationParams pagination)
    {
        var query = _context.Usuarios
            .Include(u => u.Role)
            .AsQueryable();

        // --- FILTROS ---
        if (!string.IsNullOrWhiteSpace(filter.Busqueda))
        {
            var b = filter.Busqueda.ToLower();
            query = query.Where(u => u.Nombre.ToLower().Contains(b) 
                                  || u.Apellidos.ToLower().Contains(b) 
                                  || u.Correo.ToLower().Contains(b));
        }

        // ... otros filtros (RoleId, Activo)

        // 1. CONTEO (Antes de ordenar y paginar para que sea más rápido)
        var totalRecords = await query.CountAsync();

        // 2. ORDENAMIENTO (🔥 Los más nuevos primero)
        // Usamos la fecha de creación que ya tienes en tu BaseEntity
        query = query.OrderByDescending(u => u.FechaCreacion);

        // 3. PAGINACIÓN
        var usuarios = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Paged(usuarios.ToResponseList(), pagination, totalRecords, "Usuarios recuperados.");
    }
    /// <summary>
    /// Obtiene el detalle de un usuario específico por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [AuthLvl("u", 1)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) return Error("Usuario no encontrado.");

        return Result(usuario.ToResponse(), "Detalle del usuario recuperado.");
    }

    /// <summary>
    /// Cambia el rol de un usuario en el sistema.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="newRoleId">ID del nuevo rol a asignar.</param>
    [HttpPut("{id}/role")]
    [AuthLvl("u", 3)] // Nivel 3: Gestión de seguridad de usuarios
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Activa o desactiva a un usuario (Bloqueo de acceso).
    /// </summary>
    /// <remarks>
    /// Un usuario desactivado no podrá iniciar sesión aunque su contraseña sea correcta.
    /// </remarks>
    [HttpPatch("{id}/status")]
    [AuthLvl("u", 2)] // Nivel 2: Edición de estados
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return Error("Usuario no encontrado.");

        // Invertimos el estado actual
        usuario.Activo = !usuario.Activo;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        string estado = usuario.Activo ? "activado" : "desactivado";
        return Result(usuario.Activo, $"El usuario ha sido {estado} correctamente.");
    }

   /// <summary>
    /// Obtiene un resumen estadístico de los usuarios en una sola consulta.
    /// </summary>
    [HttpGet("stats")]
    [AuthLvl("u", 1)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        // Una sola consulta agrupada para no castigar a la base de datos
        var stats = await _context.Usuarios
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Activos = g.Count(u => u.Activo),
                Inactivos = g.Count(u => !u.Activo)
            })
            .FirstOrDefaultAsync();

        // Manejo de caso para tabla vacía
        var data = stats ?? new { Total = 0, Activos = 0, Inactivos = 0 };

        // 🔥 Usamos tu método Result para que el JSON lleve Success = true
        return Result(data, "Estadísticas recuperadas exitosamente.");
    }
}