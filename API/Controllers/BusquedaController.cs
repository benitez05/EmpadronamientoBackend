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
    [AuthLvl("b", 1)] 
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
                .Include(p => p.Fotos) // 🔥 Cambiamos Caras por Fotos
                .Include(p => p.Empadronamientos).ThenInclude(ep => ep.Empadronamiento)
                .Where(p => idsABuscar.Contains(p.Id) && p.OrganizacionId == orgId)
                .ToListAsync();

            // 5. Mapear a DTOs
            var resultados = new List<BusquedaBiometricaBasicaResponse>();

            foreach (var p in personasBasicas)
            {
                // 🔥 Obtenemos la foto principal filtrando por su tipo, igual que en tu resumen
                var fotoPrincipal = p.Fotos.FirstOrDefault(f => f.Tipo == "Biométrico Facial")
                                 ?? p.Fotos.FirstOrDefault();

                // 🔥 Usamos GetPreSignedUrl en lugar de GetFileUrl
                string? fotoUrl = fotoPrincipal != null ? _s3Service.GetPreSignedUrl(fotoPrincipal.S3Key) : null;

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
    [HttpGet("{personaId}/basico")]
    [AuthLvl("b", 1)]
    [EndpointSummary("Obtener información básica de una persona (Datos, Direcciones, Familiares y Redes)")]
    public async Task<IActionResult> ObtenerDatosBasicosPersona(int personaId)
    {
        try
        {
            // 1. Consulta ligera: Solo traemos las 3 relaciones solicitadas
            var p = await _context.Personas
                .Include(x => x.Direcciones)
                .Include(x => x.Familiares)
                .Include(x => x.RedesSociales)
                .FirstOrDefaultAsync(x => x.Id == personaId);

            if (p == null)
                return Error("Persona no encontrada o sin acceso.", "PERSONA_NOT_FOUND");

            // 2. Mapeo a la respuesta simplificada
            var datosBasicos = new PersonaBasicoResponse
            {
                PersonaId = p.Id,
                Nombre = p.Nombre ?? "",
                ApellidoPaterno = p.ApellidoPaterno ?? "",
                ApellidoMaterno = p.ApellidoMaterno ?? "",
                Apodo = p.Apodo,
                Sexo = p.Sexo,
                Edad = p.Edad,
                Estatura = p.Estatura,
                FechaNacimiento = p.FechaNacimiento?.ToString("yyyy-MM-dd"),
                Nacionalidad = p.Nacionalidad,
                Originario = p.Originario,
                Telefono = p.Telefono,
                EstadoCivil = p.EstadoCivil,
                Escolaridad = p.Escolaridad,
                OficioProfesion = p.OficioProfesion,
                ObservacionesGenerales = p.ObservacionesGenerales,

                // Direcciones Personales (USANDO EL DTO CON ID)
                Direcciones = p.Direcciones.Select(d => new DireccionDto
                {
                    Id = d.Id, // 🔥 Asegurado el mapeo del ID
                    Calle = d.Calle ?? "",
                    NumeroExterior = d.NumeroExterior ?? "",
                    NumeroInterior = d.NumeroInterior ?? "",
                    Cp = d.CP,
                    Colonia = d.Colonia ?? "",
                    Municipio = d.Municipio ?? "",
                    Estado = d.Estado ?? "",
                    Pais = d.Pais ?? "",
                    Referencia = d.Referencia,
                    Latitud = d.Latitud,
                    Longitud = d.Longitud,
                    EsPrincipal = d.EsPrincipal
                }).ToList(),

                // Familiares (USANDO EL DTO CON ID)
                Familiares = p.Familiares.Select(f => new FamiliarDto
                {
                    Id = f.Id, // 🔥 Asegurado el mapeo del ID
                    NombreCompleto = f.NombreCompleto ?? "",
                    Parentesco = f.Parentesco ?? "N/A",
                    Telefono = f.Telefono,
                    Direccion = f.Direccion
                }).ToList(),

                // Redes Sociales (USANDO EL DTO CON ID)
                RedesSociales = p.RedesSociales.Select(rs => new RedSocialDto
                {
                    Id = rs.Id, // 🔥 Asegurado el mapeo del ID
                    TipoRedSocial = rs.TipoRedSocial ?? "",
                    Usuario = rs.Usuario ?? "",
                    UrlPerfil = rs.UrlPerfil
                }).ToList()
            };

            return Result(datosBasicos, "Datos básicos recuperados exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar datos básicos de persona {PersonaId}", personaId);
            return Error("Ocurrió un problema al consultar la información de la persona.", "BASIC_INFO_ERROR");
        }
    }

    [HttpGet("{personaId}/resumen")]
    [AuthLvl("b", 1)]
    [EndpointSummary("Obtener el expediente completo y detallado de una persona por su ID")]
    public async Task<IActionResult> ObtenerResumenPersona(int personaId)
    {
        try
        {
            // 1. Consulta profunda: Traemos todo el árbol
            var p = await _context.Personas
                // .Include(x => x.Caras) // Comentado/Eliminado si ya arreglaste lo de Foto vs Cara
                .Include(x => x.Fotos)
                .Include(x => x.Direcciones)
                .Include(x => x.Familiares)
                .Include(x => x.RedesSociales)
                .Include(x => x.Empadronamientos)
                    .ThenInclude(ep => ep.Empadronamiento)
                        .ThenInclude(e => e.Lugar)
                .FirstOrDefaultAsync(x => x.Id == personaId);

            if (p == null)
                return Error("Expediente no encontrado o sin acceso.", "PERSONA_NOT_FOUND");

            var fotoPrincipal = p.Fotos.FirstOrDefault(f => f.Tipo == "Biométrico Facial")
                               ?? p.Fotos.FirstOrDefault();

            // 2. Mapeo a la respuesta enriquecida
            var expediente = new PersonaResumenResponse
            {
                PersonaId = p.Id,
                Nombre = p.Nombre ?? "",
                ApellidoPaterno = p.ApellidoPaterno ?? "",
                ApellidoMaterno = p.ApellidoMaterno ?? "",
                Apodo = p.Apodo,
                Sexo = p.Sexo,
                Edad = p.Edad,
                Estatura = p.Estatura,
                FechaNacimiento = p.FechaNacimiento?.ToString("yyyy-MM-dd"),
                Nacionalidad = p.Nacionalidad,
                Originario = p.Originario,
                Telefono = p.Telefono,
                EstadoCivil = p.EstadoCivil,
                Escolaridad = p.Escolaridad,
                OficioProfesion = p.OficioProfesion,
                ObservacionesGenerales = p.ObservacionesGenerales,

                Rostro = fotoPrincipal != null ? new RostroDto { FotoId = fotoPrincipal.Id } : null,
                FotoPrincipalUrl = fotoPrincipal != null ? _s3Service.GetPreSignedUrl(fotoPrincipal.S3Key) : null,

                // Direcciones Personales (USANDO EL MISMO DTO CON ID)
                Direcciones = p.Direcciones.Select(d => new DireccionDto
                {
                    Id = d.Id, // 🔥 Ahora mapea el ID también en el resumen completo
                    Calle = d.Calle ?? "",
                    NumeroExterior = d.NumeroExterior ?? "",
                    NumeroInterior = d.NumeroInterior ?? "",
                    Cp = d.CP,
                    Colonia = d.Colonia ?? "",
                    Municipio = d.Municipio ?? "",
                    Estado = d.Estado ?? "",
                    Pais = d.Pais ?? "",
                    Referencia = d.Referencia,
                    Latitud = d.Latitud,
                    Longitud = d.Longitud,
                    EsPrincipal = d.EsPrincipal
                }).ToList(),

                // Familiares (USANDO EL MISMO DTO CON ID)
                Familiares = p.Familiares.Select(f => new FamiliarDto
                {
                    Id = f.Id, // 🔥 Ahora mapea el ID también en el resumen completo
                    NombreCompleto = f.NombreCompleto ?? "",
                    Parentesco = f.Parentesco ?? "N/A",
                    Telefono = f.Telefono,
                    Direccion = f.Direccion
                }).ToList(),

                // Redes Sociales (USANDO EL MISMO DTO CON ID)
                RedesSociales = p.RedesSociales.Select(rs => new RedSocialDto
                {
                    Id = rs.Id, // 🔥 Ahora mapea el ID también en el resumen completo
                    TipoRedSocial = rs.TipoRedSocial ?? "",
                    Usuario = rs.Usuario ?? "",
                    UrlPerfil = rs.UrlPerfil
                }).ToList(),

                Fotos = p.Fotos.Select(f => new FotoDto
                {
                    FotoId = f.Id,
                    TipoFoto = f.Tipo ?? "Otro",
                    Descripcion = f.Descripcion,
                    Url = _s3Service.GetPreSignedUrl(f.S3Key)
                }).ToList(),

                // Empadronamientos (El evento)
                Empadronamientos = p.Empadronamientos
                    .Where(ep => ep.Empadronamiento != null)
                    .Select(ep => new EmpadronamientoHistorialDto
                    {
                        Id = ep.Empadronamiento!.Id,
                        Folio = ep.Empadronamiento.Folio ?? "S/F",
                        Fecha = ep.Empadronamiento.Fecha.ToString("yyyy-MM-dd"),
                        Hora = ep.Empadronamiento.Hora.ToString(@"hh\:mm"),
                        Crpn = ep.Empadronamiento.CRP ?? "",
                        NarrativaHechos = ep.Empadronamiento.NarrativaHechos ?? "",

                        Lugar = ep.Empadronamiento.Lugar != null ? new LugarDto
                        {
                            Calle = ep.Empadronamiento.Lugar.Calle ?? "",
                            NumeroExterior = ep.Empadronamiento.Lugar.NumeroExterior ?? "",
                            NumeroInterior = ep.Empadronamiento.Lugar.NumeroInterior ?? "",
                            Cp = ep.Empadronamiento.Lugar.CP,
                            Colonia = ep.Empadronamiento.Lugar.Colonia ?? "",
                            Municipio = ep.Empadronamiento.Lugar.Municipio ?? "",
                            Estado = ep.Empadronamiento.Lugar.Estado ?? "",
                            Referencia = ep.Empadronamiento.Lugar.Referencia,
                            Latitud = ep.Empadronamiento.Lugar.Latitud,
                            Longitud = ep.Empadronamiento.Lugar.Longitud,
                            ImagenID = ep.Empadronamiento.Lugar.ImagenId > 0 ? ep.Empadronamiento.Lugar.ImagenId : null
                        } : null
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