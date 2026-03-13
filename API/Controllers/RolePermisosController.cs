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