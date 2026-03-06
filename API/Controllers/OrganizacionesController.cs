using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using EmpadronamientoBackend.Application.DTOs;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrganizacionesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OrganizacionesController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private async Task<int> GetMaestraId() =>
        (await _context.ConfiguracionesGlobales.AsNoTracking().FirstOrDefaultAsync())?.OrganizacionMaestraId ?? -1;

    [HttpGet]
    [AuthLvl("o", 1)]
    [EndpointSummary("Listar organizaciones")]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizacionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        // El filtro global del DbContext ya maneja el bypass si eres Tipo 4.
        var orgs = await _context.Organizaciones
            .Include(o => o.Plan)
            .Select(o => new OrganizacionResponse(
                o.Id, o.Nombre, o.Descripcion, o.EmailContacto, o.Telefono,
                o.Pais, o.Ciudad, o.LogoUrl, o.Activa, o.FechaVencimiento, o.Plan.Nombre
            ))
            .ToListAsync();

        return Result(orgs, "Organizaciones recuperadas exitosamente.");
    }

    [HttpPost]
    [AuthLvl("o", 3)]
    [EndpointSummary("Crear nueva organización")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] OrganizacionRequest request)
    {
        // 1. Usar la estrategia de ejecución para soportar transacciones con RetryingStrategy
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Verificamos si el nombre existe (usando IgnoreQueryFilters para duplicados globales)
            if (await _context.Organizaciones.IgnoreQueryFilters().AnyAsync(o => o.Nombre == request.Nombre))
                return Error("El nombre de la organización ya existe.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaOrg = new Organizacion
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    EmailContacto = request.EmailContacto,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion,
                    Pais = request.Pais,
                    Ciudad = request.Ciudad,
                    LogoUrl = request.LogoUrl,
                    PlanId = request.PlanId,
                    FechaVencimiento = request.FechaVencimiento,
                    Activa = request.Activa
                };

                _context.Organizaciones.Add(nuevaOrg);
                await _context.SaveChangesAsync();

                // 2. Activar módulos base (u = usuarios, r = roles)
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

                // 3. Crear los 3 Roles Maestro
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
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Error("Error crítico al configurar los roles base de la organización.");
            }
        });
    }

    [HttpPut("{id}")]
    [AuthLvl("o", 3)]
    [EndpointSummary("Editar organización")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] OrganizacionRequest request)
    {
        // Buscamos la org. Si eres Tipo 4, el filtro global te permitirá encontrar cualquiera.
        var org = await _context.Organizaciones.FirstOrDefaultAsync(o => o.Id == id);

        if (org == null) return Error("Organización no encontrada.");

        org.Nombre = request.Nombre;
        org.Descripcion = request.Descripcion;
        org.EmailContacto = request.EmailContacto;
        org.Telefono = request.Telefono;
        org.Direccion = request.Direccion;
        org.Pais = request.Pais;
        org.Ciudad = request.Ciudad;
        org.LogoUrl = request.LogoUrl;
        org.PlanId = request.PlanId;
        org.Activa = request.Activa;
        org.FechaVencimiento = request.FechaVencimiento;

        await _context.SaveChangesAsync();
        return Result("Actualizado", "Organización actualizada con éxito.");
    }
}