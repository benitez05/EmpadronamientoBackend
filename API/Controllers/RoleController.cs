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


    [HttpPut("{id}")]
    [AuthLvl("r", 3)]
    [EndpointSummary("Editar un rol")]
    [EndpointDescription("Actualiza la información básica de un rol (como su nombre), validando que no exista un duplicado en la organización.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole([FromRoute] int id, [FromBody] RoleUpdateRequestDto request)
    {
        // 1. Buscar el rol a editar
        var role = await _context.Roles.FindAsync(id);
        if (role == null) 
            return Error("El rol especificado no existe.");

        // 2. Limpiar el texto (quitar espacios en blanco al inicio y al final)
        var nuevoNombre = request.Nombre.Trim();

        // 3. Validar si el usuario realmente cambió el nombre
        // Si mandó el mismo nombre que ya tenía, evitamos hacer la consulta a la BD
        if (role.Nombre.Equals(nuevoNombre, StringComparison.OrdinalIgnoreCase))
        {
            return Result(true, "El rol fue actualizado (sin cambios en el nombre).");
        }

        // 4. Validar que el nombre no esté repetido en OTRA entidad de la misma organización
        var nombreRepetido = await _context.Roles
            .AnyAsync(r => r.Id != id && r.Nombre.ToLower() == nuevoNombre.ToLower());

        if (nombreRepetido) 
            return Error($"Ya existe un rol con el nombre '{nuevoNombre}' en la organización.");

        // 5. Actualizar los datos
        role.Nombre = nuevoNombre;

        // Nota: Como tu entidad hereda de AuditoriaEntidad, tu interceptor o 
        // método SaveChanges() seguramente ya se encarga de actualizar la FechaActualizacion.
        await _context.SaveChangesAsync();

        return Result(true, "Rol actualizado correctamente.");
    }

    [HttpPost("reassign-delete")]
    [AuthLvl("r", 3)]
    [EndpointSummary("Eliminar rol y reasignar usuarios (Mediante DTO)")]
    [EndpointDescription("Reasigna todos los usuarios del rol a eliminar hacia un rol sustituto y luego borra el rol original.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAndReassign([FromBody] RoleDeleteRequestDto request)
    {
        // 1. Validar lógica de negocio
        if (request.RoleIdAEliminar == request.RoleIdSustituto)
            return Error("El rol a eliminar y el rol sustituto no pueden ser el mismo.");

        // 2. Buscar el rol a eliminar
        var roleAEliminar = await _context.Roles.FindAsync(request.RoleIdAEliminar);
        if (roleAEliminar == null) 
            return Error("El rol que deseas eliminar no fue encontrado.");

        // 3. Validar que el rol sustituto exista en el tenant
        var roleSustitutoExiste = await _context.Roles.AnyAsync(r => r.Id == request.RoleIdSustituto);
        if (!roleSustitutoExiste) 
            return Error("El rol sustituto especificado no existe.");

        // 4. MIGRACIÓN MASIVA ULTRA RÁPIDA (ExecuteUpdateAsync)
        await _context.Usuarios
            .Where(u => u.RoleId == request.RoleIdAEliminar)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.RoleId, request.RoleIdSustituto));

        // 5. Eliminar el rol original de la base de datos
        _context.Roles.Remove(roleAEliminar);
        await _context.SaveChangesAsync();

        return Result(true, "Rol eliminado y usuarios reasignados correctamente.");
    }
}