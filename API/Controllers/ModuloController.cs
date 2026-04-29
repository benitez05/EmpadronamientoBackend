using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Infrastructure.Persistence;
using EmpadronamientoBackend.Application.Interfaces;
using BenitezLabs.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModulosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IS3Service _s3Service;

    public ModulosController(ApplicationDbContext context, ICurrentUserService currentUser, IS3Service s3Service)
    {
        _context = context;
        _currentUser = currentUser;
        _s3Service = s3Service;
    }

    [HttpGet]
    [Authorize]
    [EndpointSummary("Listado completo de módulos con URLs de iconos")]
    public async Task<IActionResult> GetAll()
    {
        var modulos = await _context.Modulos
              .IgnoreQueryFilters() 
              .OrderBy(m => m.Nombre)
              .ToListAsync();

        // El mapper ToResponseList internamente usará s3Service.GetFileUrl
        return Result(modulos.ToResponseList(_s3Service), "Módulos recuperados exitosamente.");
    }

    [HttpPost]
    [AuthLvl("m", 2)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Registrar nuevo módulo")]
    public async Task<IActionResult> Create([FromForm] ModuloRequest request)
    {
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para crear módulos del sistema.");

        if (await _context.Modulos.AnyAsync(m => m.K == request.K))
            return Error($"La clave '{request.K}' ya está en uso.");

        var nuevoModulo = request.ToEntity();
        
        if (request.IconoArchivo != null)
        {
            // 1. Definimos la subcarpeta funcional del sistema
            var pathSugerido = $"Sistema/Modulos/{Guid.NewGuid()}{Path.GetExtension(request.IconoArchivo.FileName)}";
            
            // 2. El servicio le clava el "dev/" o "prod/" y nos regresa la ruta completa (ej: dev/Sistema/Modulos/uuid.png)
            var keyFinal = await _s3Service.UploadImageAsync(request.IconoArchivo.OpenReadStream(), pathSugerido, request.IconoArchivo.ContentType);
            
            // 3. Guardamos la KEY COMPLETA en la base de datos
            nuevoModulo.Icono = keyFinal;
        }

        _context.Modulos.Add(nuevoModulo);
        await _context.SaveChangesAsync();

        return Result(nuevoModulo.ToResponse(_s3Service), "Módulo creado correctamente.");
    }

    [HttpPut("{id}")]
    [AuthLvl("m", 2)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Actualizar módulo e icono")]
    public async Task<IActionResult> Update(int id, [FromForm] ModuloRequest request)
    {
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para modificar módulos.");

        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        if (modulo.K != request.K && await _context.Modulos.AnyAsync(m => m.K == request.K))
            return Error($"La clave '{request.K}' ya está siendo usada.");

        modulo.Nombre = request.Nombre;
        modulo.K = request.K;
        modulo.Color = request.Color ?? "#6B7280";

        if (request.IconoArchivo != null)
        {
            // Eliminamos el icono anterior si existía (opcional, pero recomendado para no llenar de basura S3)
            if (!string.IsNullOrEmpty(modulo.Icono))
            {
                await _s3Service.DeleteImageAsync(modulo.Icono);
            }

            var pathSugerido = $"Sistema/Modulos/{Guid.NewGuid()}{Path.GetExtension(request.IconoArchivo.FileName)}";
            
            // Subimos y obtenemos la key con el prefijo de entorno
            var keyFinal = await _s3Service.UploadImageAsync(request.IconoArchivo.OpenReadStream(), pathSugerido, request.IconoArchivo.ContentType);
            
            modulo.Icono = keyFinal;
        }

        await _context.SaveChangesAsync();
        return Result(modulo.ToResponse(_s3Service), "Módulo actualizado con éxito.");
    }

    [HttpDelete("{id}")]
    [AuthLvl("m", 3)]
    [EndpointSummary("Eliminar módulo del sistema")]
    public async Task<IActionResult> Delete(int id)
    {
        if (_currentUser.Tipo < 3)
            return Error("No tienes permiso para eliminar módulos.");

        var modulo = await _context.Modulos.FindAsync(id);
        if (modulo == null) return Error("Módulo no encontrado.");

        var tieneDependencias = await _context.RolesPermisos.AnyAsync(rp => rp.ModuloId == id);
        if (tieneDependencias)
            return Error("No se puede eliminar: existen roles con permisos asignados.");

        // Eliminamos el icono de S3 antes de borrar el registro
        if (!string.IsNullOrEmpty(modulo.Icono))
        {
            await _s3Service.DeleteImageAsync(modulo.Icono);
        }

        _context.Modulos.Remove(modulo);
        await _context.SaveChangesAsync();

        return Result(true, "Módulo eliminado correctamente.");
    }
}