using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using EmpadronamientoBackend.Infrastructure.Persistence;
using BenitezLabs.Domain.Entities.Catalogos;
using BenitezLabs.API.Authorization;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CatalogosController : BaseController
{
    private readonly ApplicationDbContext _context;

    // Ya ni siquiera necesitamos inyectar ICurrentUserService porque el DbContext hace el trabajo
    public CatalogosController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region LECTURA DE CATÁLOGOS (SISTEMA)

    [HttpGet]
    [EndpointSummary("Obtiene todos los catálogos del sistema incluyendo sus elementos activos")]
    public async Task<IActionResult> GetCatalogos()
    {
        // Nota: Gracias al filtro global, c.Items ya viene filtrado por OrganizacionId automáticamente
        var catalogos = await _context.Catalogos
            .Select(c => new CatalogoResponse
            {
                Id = c.Id,
                Clave = c.Clave,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                // Solo nos preocupamos por filtrar los activos y ordenarlos
                Items = c.Items
                    .Where(i => i.Activo)
                    .OrderBy(i => i.Orden)
                    .Select(i => new CatalogoItemResponse
                    {
                        Id = i.Id,
                        Nombre = i.Nombre,
                        Codigo = i.Codigo,
                        Orden = i.Orden,
                        Activo = i.Activo
                    }).ToList()
            })
            .ToListAsync();

        return Result(catalogos);
    }

    [HttpGet("{clave}")]
    [EndpointSummary("Obtiene un catálogo y sus items activos buscando por su Clave (Ej: ESTADO_CIVIL)")]
    public async Task<IActionResult> GetCatalogoByClave(string clave)
    {
        var catalogo = await _context.Catalogos
            .Include(c => c.Items
                .Where(i => i.Activo)
                .OrderBy(i => i.Orden))
            .FirstOrDefaultAsync(c => c.Clave == clave.ToUpper());

        if (catalogo == null)
            return Error($"No se encontró el catálogo con la clave '{clave}'.");

        var response = new CatalogoResponse
        {
            Id = catalogo.Id,
            Clave = catalogo.Clave,
            Nombre = catalogo.Nombre,
            Descripcion = catalogo.Descripcion,
            Items = catalogo.Items.Select(i => new CatalogoItemResponse
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Codigo = i.Codigo,
                Orden = i.Orden,
                Activo = i.Activo
            }).ToList()
        };

        return Result(response);
    }

    #endregion

    #region CRUD DE ITEMS DE CATÁLOGO

    [AuthLvl("cat", 2)]
    [HttpPost("{catalogoId}/items")]
    [EndpointSummary("Agrega un nuevo elemento a un catálogo existente")]
    public async Task<IActionResult> AddCatalogoItem(int catalogoId, [FromBody] CreateCatalogoItemRequest request)
    {
        if (!await _context.Catalogos.AnyAsync(c => c.Id == catalogoId))
            return Error("El catálogo especificado no existe en el sistema.");

        // 🚨 1. VALIDACIÓN DE ORDEN
        bool ordenOcupado = await _context.CatalogoItems
            .AnyAsync(i => i.CatalogoId == catalogoId && i.Orden == request.Orden && i.Activo);

        if (ordenOcupado)
            return Error($"Ya existe un elemento activo con el orden {request.Orden} en este catálogo.");

        // 🚨 2. VALIDACIÓN DE NOMBRE
        bool nombreOcupado = await _context.CatalogoItems
            .AnyAsync(i => i.CatalogoId == catalogoId && i.Nombre.ToLower() == request.Nombre.ToLower() && i.Activo);

        if (nombreOcupado)
            return Error($"Ya existe un elemento activo con el nombre '{request.Nombre}' en este catálogo.");

        // 🚨 3. VALIDACIÓN DE CÓDIGO (Solo si enviaron uno)
        if (!string.IsNullOrWhiteSpace(request.Codigo))
        {
            bool codigoOcupado = await _context.CatalogoItems
                .AnyAsync(i => i.CatalogoId == catalogoId && i.Codigo != null && i.Codigo.ToLower() == request.Codigo.ToLower() && i.Activo);

            if (codigoOcupado)
                return Error($"Ya existe un elemento activo con el código '{request.Codigo}' en este catálogo.");
        }

        var item = new CatalogoItem
        {
            Nombre = request.Nombre,
            Codigo = request.Codigo,
            Orden = request.Orden,
            Activo = true,
            CatalogoId = catalogoId
            // OrganizacionId se asignará mágicamente en SaveChangesAsync()
        };

        _context.CatalogoItems.Add(item);
        await _context.SaveChangesAsync();

        return Result(item.Id, "Elemento agregado al catálogo exitosamente.");
    }

    [AuthLvl("cat", 2)]
    [HttpPut("items/{itemId}")]
    [EndpointSummary("Actualiza un elemento específico del catálogo")]
    public async Task<IActionResult> UpdateCatalogoItem(int itemId, [FromBody] UpdateCatalogoItemRequest request)
    {
        var item = await _context.CatalogoItems.FindAsync(itemId);

        if (item == null)
            return Error("Elemento no encontrado o no tienes permiso para editarlo.");

        // 🚨 1. VALIDACIÓN DE ORDEN
        if (item.Orden != request.Orden)
        {
            bool ordenOcupado = await _context.CatalogoItems
                .AnyAsync(i => i.CatalogoId == item.CatalogoId && i.Id != itemId && i.Orden == request.Orden && i.Activo);

            if (ordenOcupado)
                return Error($"Ya existe otro elemento activo con el orden {request.Orden} en este catálogo.");
        }

        // 🚨 2. VALIDACIÓN DE NOMBRE
        if (item.Nombre.ToLower() != request.Nombre.ToLower())
        {
            bool nombreOcupado = await _context.CatalogoItems
                .AnyAsync(i => i.CatalogoId == item.CatalogoId && i.Id != itemId && i.Nombre.ToLower() == request.Nombre.ToLower() && i.Activo);

            if (nombreOcupado)
                return Error($"Ya existe otro elemento activo con el nombre '{request.Nombre}' en este catálogo.");
        }

        // 🚨 3. VALIDACIÓN DE CÓDIGO (Solo si enviaron uno y si lo cambiaron)
        if (!string.IsNullOrWhiteSpace(request.Codigo) && (item.Codigo?.ToLower() != request.Codigo.ToLower()))
        {
            bool codigoOcupado = await _context.CatalogoItems
                .AnyAsync(i => i.CatalogoId == item.CatalogoId && i.Id != itemId && i.Codigo != null && i.Codigo.ToLower() == request.Codigo.ToLower() && i.Activo);

            if (codigoOcupado)
                return Error($"Ya existe otro elemento activo con el código '{request.Codigo}' en este catálogo.");
        }

        item.Nombre = request.Nombre;
        item.Codigo = request.Codigo;
        item.Orden = request.Orden;
        item.Activo = request.Activo;

        await _context.SaveChangesAsync();

        return Result(true, "Elemento actualizado exitosamente.");
    }

    [AuthLvl("cat", 2)]
    [HttpDelete("items/{itemId}")]
    [EndpointSummary("Realiza un borrado lógico (desactiva) un elemento del catálogo")]
    public async Task<IActionResult> DeleteCatalogoItem(int itemId)
    {
        var item = await _context.CatalogoItems.FindAsync(itemId);

        if (item == null)
            return Error("Elemento no encontrado o no tienes permiso para eliminarlo.");

        // Borrado lógico
        item.Activo = false;
        await _context.SaveChangesAsync();

        return Result(true, "Elemento desactivado exitosamente.");
    }

    #endregion
}