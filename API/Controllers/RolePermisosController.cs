using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Infrastructure.Persistence;
using BenitezLabs.Domain.Entities;

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

    [HttpGet("{roleId}/modulos")]
    [AuthLvl("r", 3)]
    [EndpointSummary("Listar módulos y permisos por rol")]
    [EndpointDescription("Obtiene los módulos del sistema y detalla el nivel de acceso del rol (0 = Sin acceso).")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ModuloPermisoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModulesPermissionsByRole([FromRoute] int roleId)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
        if (!roleExists) return Error("El rol especificado no existe.");

        var query = await _context.Modulos
            .AsNoTracking()
            .Select(m => new
            {
                Modulo = m,
                Permiso = _context.RolesPermisos
                            .FirstOrDefault(rp => rp.RoleId == roleId && rp.ModuloId == m.Id)
            })
            .ToListAsync();

        var result = query.Select(x => new ModuloPermisoResponseDto
        {
            ModuloId = x.Modulo.Id,
            Nombre = x.Modulo.Nombre,
            K = x.Modulo.K,
            Color = x.Modulo.Color,
            Multinivel = x.Modulo.Multinivel,
            NivelAcceso = x.Permiso != null ? x.Permiso.Lvl : 0
        }).ToList();

        return Result(result, "Lista de módulos y permisos obtenida correctamente.");
    }

    [HttpPut("{roleId}/permisos")]
    [AuthLvl("r", 3)] // Ajusta el permiso de seguridad para escritura si es necesario
    [EndpointSummary("Actualizar permisos del rol de forma masiva")]
    [EndpointDescription("Sincroniza los permisos de un rol. Si el nivel es 0 se elimina el acceso, si es mayor a 0 se crea o se actualiza.")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(
        [FromRoute] int roleId, 
        [FromBody] List<ModuloPermisoUpdateRequestDto> request)
    {
        // 1. Validar que el rol exista dentro del tenant actual
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
        if (!roleExists) return Error("El rol especificado no existe.");

        // 2. Obtener los permisos actuales en la base de datos para este rol
        var permisosActuales = await _context.RolesPermisos
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        // 3. Procesar el listado enviado por el cliente
        foreach (var item in request)
        {
            // Buscamos si el rol ya tenía un permiso previo en este módulo
            var permisoExistente = permisosActuales
                .FirstOrDefault(rp => rp.ModuloId == item.ModuloId);

            if (item.NivelAcceso == 0)
            {
                // REGLA: Si viene nivel 0 y existe en la base de datos, se remueve
                if (permisoExistente != null)
                {
                    _context.RolesPermisos.Remove(permisoExistente);
                }
            }
            else
            {
                // REGLA: Si viene nivel > 0 (1, 2, 3), se actualiza o se crea
                if (permisoExistente != null)
                {
                    // Si cambió el nivel, lo actualizamos
                    permisoExistente.Lvl = item.NivelAcceso;
                }
                else
                {
                    // Si no existía la relación, creamos el nuevo registro
                    var nuevoPermiso = new RolePermiso
                    {
                        RoleId = roleId,
                        ModuloId = item.ModuloId,
                        Lvl = item.NivelAcceso
                    };
                    await _context.RolesPermisos.AddAsync(nuevoPermiso);
                }
            }
        }

        // 4. Guardar todos los cambios de forma transaccional
        await _context.SaveChangesAsync();

        return Result(true, "Permisos del rol actualizados exitosamente.");
    }
}