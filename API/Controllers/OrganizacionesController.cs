using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using EmpadronamientoBackend.Application.Mappers;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrganizacionesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IS3Service _s3Service;

    public OrganizacionesController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IS3Service s3Service)
    {
        _context = context;
        _currentUser = currentUser;
        _s3Service = s3Service;
    }

    [HttpGet]
    [AuthLvl("o", 1)]
    [EndpointSummary("Listar organizaciones")]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizacionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var orgs = await _context.Organizaciones
            .Include(o => o.Plan)
            .ToListAsync();

        return Result(orgs.ToResponseList(_s3Service), "Organizaciones recuperadas exitosamente.");
    }

    [HttpPost]
    [AuthLvl("o", 3)]
    [EndpointSummary("Crear nueva organización")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromForm] OrganizacionRequest request)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            if (await _context.Organizaciones.IgnoreQueryFilters()
                .AnyAsync(o => o.Nombre == request.Nombre))
                return Error("El nombre de la organización ya existe.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string? logoPath = null;

                // Subida opcional del logo
                if (request.Logo != null && request.Logo.Length > 0)
                {
                    try
                    {
                        var fileName = $"organizaciones/{DateTime.UtcNow.Ticks}_{request.Logo.FileName}";
                        using var stream = request.Logo.OpenReadStream();

                        await _s3Service.UploadImageAsync(stream, fileName, request.Logo.ContentType);
                        logoPath = fileName;
                    }
                    catch
                    {
                        return Error("No se pudo subir el logo de la organización.");
                    }
                }

                var nuevaOrg = new Organizacion
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    EmailContacto = request.EmailContacto,
                    Telefono = request.Telefono,

                    Calle = request.Calle,
                    NumeroExterior = request.NumeroExterior,
                    NumeroInterior = request.NumeroInterior,
                    CP = request.CP,
                    Colonia = request.Colonia,
                    Municipio = request.Municipio,
                    Estado = request.Estado,
                    Pais = request.Pais,

                    LogoUrl = logoPath,
                    PlanId = request.PlanId,
                    FechaVencimiento = request.FechaVencimiento,
                    Activa = request.Activa
                };

                _context.Organizaciones.Add(nuevaOrg);
                await _context.SaveChangesAsync();

                // Activar módulos base
                var modulosBase = await _context.Modulos
                    .Where(m => m.K == "u" || m.K == "r")
                    .ToListAsync();

                foreach (var m in modulosBase)
                {
                    _context.OrganizacionModulos.Add(new OrganizacionModulo
                    {
                        OrganizacionId = nuevaOrg.Id,
                        ModuloId = m.Id,
                        Activo = true,
                        FechaActivacion = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                // Crear roles base
                var templates = new List<(string Nombre, int Nivel)>
                {
                    ("Administrador", 3),
                    ("Editor", 2),
                    ("Lectura", 1)
                };

                foreach (var t in templates)
                {
                    var nuevoRol = new Role
                    {
                        Nombre = t.Nombre,
                        OrganizacionId = nuevaOrg.Id
                    };

                    _context.Roles.Add(nuevoRol);
                    await _context.SaveChangesAsync();

                    foreach (var m in modulosBase)
                    {
                        _context.RolesPermisos.Add(new RolePermiso
                        {
                            RoleId = nuevoRol.Id,
                            ModuloId = m.Id,
                            Lvl = t.Nivel
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result(nuevaOrg.Id, "Organización creada correctamente.");
            }
            catch
            {
                await transaction.RollbackAsync();
                return Error("Error crítico al configurar la organización.");
            }
        });
    }

    [HttpPut("{id}")]
    [AuthLvl("o", 3)]
    [EndpointSummary("Editar organización")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<OrganizacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromForm] OrganizacionRequest request)
    {
        var org = await _context.Organizaciones
            .Include(o => o.Plan)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (org == null)
            return Error("Organización no encontrada.");

        string? previousLogo = org.LogoUrl;

        // Subida del nuevo logo
        if (request.Logo != null && request.Logo.Length > 0)
        {
            try
            {
                var fileName = $"organizaciones/{org.Id}_{DateTime.UtcNow.Ticks}_{request.Logo.FileName}";
                using var stream = request.Logo.OpenReadStream();

                await _s3Service.UploadImageAsync(stream, fileName, request.Logo.ContentType);
                org.LogoUrl = fileName;

                if (!string.IsNullOrEmpty(previousLogo))
                {
                    try
                    {
                        await _s3Service.DeleteImageAsync(previousLogo);
                    }
                    catch
                    {
                        // no detenemos la operación si falla eliminar
                    }
                }
            }
            catch
            {
                return Error("No se pudo subir el logo proporcionado.");
            }
        }

        // Actualizar datos
        org.Nombre = request.Nombre;
        org.Descripcion = request.Descripcion;
        org.EmailContacto = request.EmailContacto;
        org.Telefono = request.Telefono;

        org.Calle = request.Calle;
        org.NumeroExterior = request.NumeroExterior;
        org.NumeroInterior = request.NumeroInterior;
        org.CP = request.CP;
        org.Colonia = request.Colonia;
        org.Municipio = request.Municipio;
        org.Estado = request.Estado;
        org.Pais = request.Pais;

        org.PlanId = request.PlanId;
        org.Activa = request.Activa;
        org.FechaVencimiento = request.FechaVencimiento;

        await _context.SaveChangesAsync();

        return Result(org.ToResponse(_s3Service), "Organización actualizada con éxito.");
    }
}