using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BenitezLabs.API.Authorization;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using EmpadronamientoBackend.Application.Mappers;
using BenitezLabs.Domain.Entities.Catalogos;

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
                // 1. CREAR ENTIDAD PRIMERO PARA OBTENER EL ID
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
                    LogoUrl = null, // Se actualiza en el siguiente paso
                    PlanId = request.PlanId,
                    FechaVencimiento = request.FechaVencimiento,
                    Activa = request.Activa
                };

                _context.Organizaciones.Add(nuevaOrg);
                await _context.SaveChangesAsync(); // 🔥 Aquí se genera nuevaOrg.Id

                // 2. SUBIR LOGO USANDO EL ID RECIÉN GENERADO
                if (request.Logo != null && request.Logo.Length > 0)
                {
                    try
                    {
                        // Estructura de carpeta física: organizaciones / {id} / archivo
                        var pathSugerido = $"organizaciones/{nuevaOrg.Id}/logo_{Guid.NewGuid()}{Path.GetExtension(request.Logo.FileName)}";
                        using var stream = request.Logo.OpenReadStream();

                        // Subimos y obtenemos la key completa (dev/organizaciones/id/...)
                        var keyFinal = await _s3Service.UploadImageAsync(stream, pathSugerido, request.Logo.ContentType);

                        // Actualizamos la entidad con la ruta real
                        nuevaOrg.LogoUrl = keyFinal;
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        return Error("No se pudo subir el logo de la organización.");
                    }
                }

                // 3. SEGUIR CON LA LÓGICA DE CATÁLOGOS Y ROLES
                await CrearCatalogosBaseAsync(nuevaOrg.Id);

                var modulosBase = await _context.Modulos
                    .Where(m => m.K == "u" || m.K == "r" || m.K == "o" || m.K == "c" || m.K == "e")
                    .ToListAsync();

                foreach (var m in modulosBase)
                {
                    _context.OrganizacionModulos.Add(new OrganizacionModulo
                    {
                        OrganizacionId = nuevaOrg.Id,
                        ModuloId = m.Id,
                        Activo = true,
                        FechaActivacion = DateTime.UtcNow,
                    });
                }

                var templates = new List<(string Nombre, int Nivel)>
                {
                    ("Administrador", 3),
                    ("Editor", 2),
                    ("Lectura", 1)
                };

                foreach (var t in templates)
                {
                    var nuevoRol = new Role { Nombre = t.Nombre, OrganizacionId = nuevaOrg.Id };
                    _context.Roles.Add(nuevoRol);
                    await _context.SaveChangesAsync();

                    foreach (var m in modulosBase)
                    {
                        _context.RolesPermisos.Add(new RolePermiso { RoleId = nuevoRol.Id, ModuloId = m.Id, Lvl = t.Nivel });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result(nuevaOrg.Id, "Organización creada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Error($"Error crítico: {ex.Message}");
            }
        });
    }

    [HttpPut("{id}")]
    [AuthLvl("o", 2)]
    [EndpointSummary("Editar organización")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] OrganizacionRequest request)
    {
        var org = await _context.Organizaciones.FirstOrDefaultAsync(o => o.Id == id);
        if (org == null) return Error("Organización no encontrada.");

        string? previousLogo = org.LogoUrl;

        if (request.Logo != null && request.Logo.Length > 0)
        {
            try
            {
                // Estructura de carpeta física para Update: organizaciones / {id} / archivo
                var pathSugerido = $"organizaciones/{org.Id}/logo_{Guid.NewGuid()}{Path.GetExtension(request.Logo.FileName)}";
                using var stream = request.Logo.OpenReadStream();

                var keyFinal = await _s3Service.UploadImageAsync(stream, pathSugerido, request.Logo.ContentType);
                org.LogoUrl = keyFinal;

                if (!string.IsNullOrEmpty(previousLogo))
                {
                    try { await _s3Service.DeleteImageAsync(previousLogo); } catch { }
                }
            }
            catch
            {
                return Error("No se pudo subir el logo proporcionado.");
            }
        }

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

    private async Task CrearCatalogosBaseAsync(int orgId)
    {
        var sys = "SYSTEM";
        var fechaSeed = DateTime.UtcNow;
        var ip = "127.0.0.1";
        var dev = "SEED";

        var catalogos = new List<Catalogo>
    {
        new Catalogo { Clave = "TIPO_RED_SOCIAL", Nombre = "Tipo de Red Social", OrganizacionId = orgId },
        new Catalogo { Clave = "ESTADO_CIVIL", Nombre = "Estado Civil", OrganizacionId = orgId },
        new Catalogo { Clave = "ESCOLARIDAD", Nombre = "Escolaridad", OrganizacionId = orgId },
        new Catalogo { Clave = "TIPO_FOTO", Nombre = "Tipo de Foto", OrganizacionId = orgId },
        new Catalogo { Clave = "PARENTESCO", Nombre = "Parentesco", OrganizacionId = orgId },
        new Catalogo { Clave = "TIPO_CARRO_RADIO_PATRULLA", Nombre = "Tipo Carro Radio Patrulla", OrganizacionId = orgId },
        new Catalogo { Clave = "OFICIO_PROFESION", Nombre = "Oficio / Profesión", OrganizacionId = orgId }
    };

        _context.Catalogos.AddRange(catalogos);
        await _context.SaveChangesAsync();

        int GetId(string clave) => catalogos.First(c => c.Clave == clave).Id;

        var items = new List<CatalogoItem>
    {
        // ================= RED SOCIAL =================
        new CatalogoItem { Nombre = "Facebook", Codigo = "FACEBOOK", Orden = 1, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Instagram", Codigo = "INSTAGRAM", Orden = 2, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "X (Twitter)", Codigo = "X", Orden = 3, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "TikTok", Codigo = "TIKTOK", Orden = 4, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "WhatsApp", Codigo = "WHATSAPP", Orden = 5, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Telegram", Codigo = "TELEGRAM", Orden = 6, CatalogoId = GetId("TIPO_RED_SOCIAL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= ESTADO CIVIL =================
        new CatalogoItem { Nombre = "Soltero", Codigo = "SOLTERO", Orden = 1, CatalogoId = GetId("ESTADO_CIVIL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Casado", Codigo = "CASADO", Orden = 2, CatalogoId = GetId("ESTADO_CIVIL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Divorciado", Codigo = "DIVORCIADO", Orden = 3, CatalogoId = GetId("ESTADO_CIVIL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Unión Libre", Codigo = "UNION_LIBRE", Orden = 4, CatalogoId = GetId("ESTADO_CIVIL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Viudo", Codigo = "VIUDO", Orden = 5, CatalogoId = GetId("ESTADO_CIVIL"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= ESCOLARIDAD =================
        new CatalogoItem { Nombre = "Sin estudios", Codigo = "SIN_ESTUDIOS", Orden = 1, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Primaria", Codigo = "PRIMARIA", Orden = 2, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Secundaria", Codigo = "SECUNDARIA", Orden = 3, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Preparatoria", Codigo = "PREPARATORIA", Orden = 4, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Técnico", Codigo = "TECNICO", Orden = 5, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Licenciatura", Codigo = "LICENCIATURA", Orden = 6, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Maestría", Codigo = "MAESTRIA", Orden = 7, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Doctorado", Codigo = "DOCTORADO", Orden = 8, CatalogoId = GetId("ESCOLARIDAD"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= TIPO FOTO =================
        new CatalogoItem { Nombre = "Rostro", Codigo = "ROSTRO", Orden = 1, CatalogoId = GetId("TIPO_FOTO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Cuerpo Completo", Codigo = "CUERPO_COMPLETO", Orden = 2, CatalogoId = GetId("TIPO_FOTO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Identificación", Codigo = "IDENTIFICACION", Orden = 3, CatalogoId = GetId("TIPO_FOTO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Señas Particulares", Codigo = "SENAS", Orden = 4, CatalogoId = GetId("TIPO_FOTO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= PARENTESCO =================
        new CatalogoItem { Nombre = "Padre", Codigo = "PADRE", Orden = 1, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Madre", Codigo = "MADRE", Orden = 2, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Hermano(a)", Codigo = "HERMANO", Orden = 3, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Pareja", Codigo = "PAREJA", Orden = 4, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Hijo(a)", Codigo = "HIJO", Orden = 5, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Otro", Codigo = "OTRO", Orden = 6, CatalogoId = GetId("PARENTESCO"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= PATRULLA =================
        new CatalogoItem { Nombre = "Sedán", Codigo = "SEDAN", Orden = 1, CatalogoId = GetId("TIPO_CARRO_RADIO_PATRULLA"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "PickUp", Codigo = "PICKUP", Orden = 2, CatalogoId = GetId("TIPO_CARRO_RADIO_PATRULLA"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Motocicleta", Codigo = "MOTO", Orden = 3, CatalogoId = GetId("TIPO_CARRO_RADIO_PATRULLA"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "SUV", Codigo = "SUV", Orden = 4, CatalogoId = GetId("TIPO_CARRO_RADIO_PATRULLA"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },

        // ================= OFICIO =================
        new CatalogoItem { Nombre = "Empleado", Codigo = "EMPLEADO", Orden = 1, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Obrero", Codigo = "OBRERO", Orden = 2, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Comerciante", Codigo = "COMERCIANTE", Orden = 3, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Estudiante", Codigo = "ESTUDIANTE", Orden = 4, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Desempleado", Codigo = "DESEMPLEADO", Orden = 5, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Chofer", Codigo = "CHOFER", Orden = 6, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Albañil", Codigo = "ALBANIL", Orden = 7, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Técnico", Codigo = "TECNICO", Orden = 8, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Profesionista", Codigo = "PROFESIONISTA", Orden = 9, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev },
        new CatalogoItem { Nombre = "Otro", Codigo = "OTRO", Orden = 10, CatalogoId = GetId("OFICIO_PROFESION"), OrganizacionId = orgId, CreadoPor = sys, FechaCreacion = fechaSeed, IpCreacion = ip, DispositivoCreacion = dev }
    };

        _context.CatalogoItems.AddRange(items);
        await _context.SaveChangesAsync();
    }
}