using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.DTOs.Responses.Busqueda;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusquedaController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IRekognitionService _rekognitionService;
    private readonly IS3Service _s3Service;
    private readonly IConfiguration _config;
    private readonly ILogger<BusquedaController> _logger;

    // 1. AGREGA EL CAMPO PRIVADO (Revisa si tu interfaz se llama ICurrentUserService u otro nombre)
    private readonly ICurrentUserService _currentUser;

    public BusquedaController(
        ApplicationDbContext context,
        IRekognitionService rekognitionService,
        IS3Service s3Service,
        IConfiguration config,
        ILogger<BusquedaController> logger,
        ICurrentUserService currentUser) // 2. AGRÉGALO AL CONSTRUCTOR
    {
        _context = context;
        _rekognitionService = rekognitionService;
        _s3Service = s3Service;
        _config = config;
        _logger = logger;

        // 3. ASÍGNSALO
        _currentUser = currentUser;
    }

    [HttpPost("biometrica")]
    [AuthLvl("b", 1)] // Ajusta la letra y nivel según tus permisos de búsqueda
    [Consumes("multipart/form-data")]
    [EndpointSummary("Paso 1: Búsqueda Biométrica Rápida (Retorna info básica de los matches)")]
    public async Task<IActionResult> BuscarPorRostro(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return Error("No se proporcionó un archivo válido para la búsqueda.", "INVALID_FILE");

        var collectionId = _config["AWS:Rekognition:Collections:Empadronamiento"];
        if (string.IsNullOrEmpty(collectionId))
            return Error("Configuración de colección biométrica no encontrada.", "CONFIG_ERROR");

        try
        {
            using var stream = archivo.OpenReadStream();

            // 1. Validar que la foto sea apta (un solo rostro, ojos abiertos, etc.)
            var validacion = await _rekognitionService.ValidateFaceAsync(stream);
            if (!validacion.IsValid)
            {
                return Error($"La foto no es apta para búsqueda: {validacion.ErrorMessage}", "INVALID_FACE");
            }

            // Reiniciar el stream para que AWS Rekognition lo lea desde el principio en el siguiente paso
            stream.Position = 0;

            // 2. Buscar coincidencias en AWS (Traemos los top 3 para descartar falsos positivos)
            var matches = await _rekognitionService.SearchFaceAsync(stream, collectionId, maxFaces: 3);

            if (!matches.Any())
            {
                return Result(new List<BusquedaBiometricaBasicaResponse>(), "No se encontraron coincidencias en la base de datos.");
            }

            // 3. Extraer los IDs de las personas del ExternalImageId (formato: "persona_123")
            var matchDict = new Dictionary<int, float>();
            foreach (var match in matches)
            {
                var partes = match.ExternalImageId.Split('_');
                if (partes.Length == 2 && int.TryParse(partes[1], out int personaId))
                {
                    // Guardamos el ID y su porcentaje de similitud (evitando duplicados de la misma persona)
                    if (!matchDict.ContainsKey(personaId))
                    {
                        matchDict.Add(personaId, match.Similarity);
                    }
                }
            }

            var idsABuscar = matchDict.Keys.ToList();

            if (!idsABuscar.Any())
            {
                return Result(new List<BusquedaBiometricaBasicaResponse>(), "Se hallaron rostros pero no se pudieron vincular a registros válidos.");
            }

            // 4. Consulta LIGERA a la Base de Datos
            int orgId = _currentUser.OrganizacionId;
            var personasBasicas = await _context.Personas
                .Include(p => p.Caras)
                .Include(p => p.Empadronamientos).ThenInclude(ep => ep.Empadronamiento)
                .Where(p => idsABuscar.Contains(p.Id) && p.OrganizacionId == orgId)
                .ToListAsync();

            // 5. Mapear a DTOs
            var resultados = new List<BusquedaBiometricaBasicaResponse>();

            foreach (var p in personasBasicas)
            {
                // Obtenemos la cara principal registrada en el sistema
                var caraPrincipal = p.Caras.FirstOrDefault();
                string? fotoUrl = caraPrincipal != null ? _s3Service.GetFileUrl(caraPrincipal.S3Key) : null;

                // Obtenemos el folio del empadronamiento más reciente
                var ultimoFolio = p.Empadronamientos
                    .OrderByDescending(e => e.EmpadronamientoId)
                    .FirstOrDefault()?.Empadronamiento?.Folio ?? "SIN REGISTRO";

                resultados.Add(new BusquedaBiometricaBasicaResponse
                {
                    PersonaId = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.ApellidoPaterno} {p.ApellidoMaterno}".Trim(),
                    Apodo = p.Apodo ?? "N/A",
                    Similitud = matchDict[p.Id],
                    FotoPrincipalUrl = fotoUrl,
                    UltimoFolio = ultimoFolio
                });
            }

            // Retornar ordenado por el que más se parece primero
            return Result(resultados.OrderByDescending(r => r.Similitud).ToList(), "Coincidencias encontradas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la búsqueda biométrica rápida.");
            return Error("Ocurrió un problema al procesar la búsqueda facial.", "BIOMETRIC_SEARCH_ERROR");
        }
    }

    [HttpGet("{personaId}/resumen")]
    [AuthLvl("b", 1)]
    [EndpointSummary("Obtener el expediente completo y detallado de una persona por su ID")]
    public async Task<IActionResult> ObtenerResumenPersona(int personaId)
    {
        try
        {
            int orgId = _currentUser.OrganizacionId;

            // 1. Consulta profunda: Traemos Persona -> Empadronamientos -> Lugar del Evento
            var p = await _context.Personas
                .Include(x => x.Caras)
                .Include(x => x.Fotos)
                .Include(x => x.Direcciones)
                .Include(x => x.Familiares)
                .Include(x => x.RedesSociales)
                .Include(x => x.Empadronamientos)
                    .ThenInclude(ep => ep.Empadronamiento)
                        .ThenInclude(e => e.Lugar) // 🔥 Necesario para las coordenadas del historial
                .FirstOrDefaultAsync(x => x.Id == personaId && x.OrganizacionId == orgId);

            if (p == null)
                return Error("Expediente no encontrado o sin acceso.", "PERSONA_NOT_FOUND");

            var fotoPrincipal = p.Fotos.FirstOrDefault(f => f.Tipo == "Biométrico Facial")
                               ?? p.Fotos.FirstOrDefault();

            var expediente = new PersonaResumenResponse
            {
                PersonaId = p.Id,
                NombreCompleto = $"{p.Nombre} {p.ApellidoPaterno} {p.ApellidoMaterno}".Trim(),
                Apodo = p.Apodo ?? "N/A",
                Sexo = p.Sexo ?? "No especificado",
                Edad = p.Edad,
                Estatura = p.Estatura,
                FechaNacimiento = p.FechaNacimiento?.ToString("yyyy-MM-dd"),
                Nacionalidad = p.Nacionalidad ?? "N/A",
                Originario = p.Originario ?? "N/A",
                Telefono = p.Telefono ?? "N/A",
                EstadoCivil = p.EstadoCivil ?? "N/A",
                Escolaridad = p.Escolaridad ?? "N/A",
                OficioProfesion = p.OficioProfesion ?? "N/A",
                ObservacionesGenerales = p.ObservacionesGenerales,

                FotoPrincipalUrl = fotoPrincipal != null ? _s3Service.GetPreSignedUrl(fotoPrincipal.S3Key) : null,

                Direcciones = p.Direcciones.Select(d => new DireccionDto
                {
                    Calle = d.Calle,
                    NumeroExterior = d.NumeroExterior,
                    NumeroInterior = d.NumeroInterior,
                    Colonia = d.Colonia,
                    Municipio = d.Municipio,
                    Estado = d.Estado,
                    Pais = d.Pais,
                    Referencia = d.Referencia,
                    Latitud = d.Latitud,
                    Longitud = d.Longitud,
                    EsPrincipal = d.EsPrincipal
                }).ToList(),

                Familiares = p.Familiares.Select(f => new FamiliarDto
                {
                    NombreCompleto = f.NombreCompleto,
                    Parentesco = f.Parentesco ?? "N/A",
                    Telefono = f.Telefono ?? "N/A",
                    Direccion = f.Direccion
                }).ToList(),

                RedesSociales = p.RedesSociales.Select(rs => new RedSocialDto
                {
                    Tipo = rs.TipoRedSocial,
                    Usuario = rs.Usuario,
                    UrlPerfil = rs.UrlPerfil
                }).ToList(),

                Fotos = p.Fotos.Select(f => new FotoDto
                {
                    Tipo = f.Tipo ?? "Otro",
                    Descripcion = f.Descripcion,
                    Url = _s3Service.GetPreSignedUrl(f.S3Key)
                }).ToList(),

                Empadronamientos = p.Empadronamientos
                    .Where(ep => ep.Empadronamiento != null)
                    .Select(ep => new EmpadronamientoHistorialDto
                    {
                        Id = ep.Empadronamiento!.Id,
                        Folio = ep.Empadronamiento.Folio ?? "S/F",
                        Fecha = ep.Empadronamiento.Fecha.ToString("yyyy-MM-dd"),
                        Hora = ep.Empadronamiento.Hora.ToString(@"hh\:mm"),
                        CRP = ep.Empadronamiento.CRP ?? "N/A",
                        Observaciones = ep.Observaciones ?? "Sin observaciones",

                        // 🔥 Mapeo de la Ubicación del Evento Histórico
                        UbicacionEvento = $"{ep.Empadronamiento.Lugar.Calle} {ep.Empadronamiento.Lugar.NumeroExterior}".Trim(),
                        LatitudEvento = ep.Empadronamiento.Lugar.Latitud,
                        LongitudEvento = ep.Empadronamiento.Lugar.Longitud
                    })
                    .OrderByDescending(e => e.Fecha)
                    .ToList()
            };

            return Result(expediente, "Expediente detallado recuperado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar resumen detallado de persona {PersonaId}", personaId);
            return Error("Ocurrió un problema al consultar el expediente completo.", "SUMMARY_ERROR");
        }
    }
}