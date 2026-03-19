using BenitezLabs.API.Authorization;
using BenitezLabs.Domain.Entities;
using BenitezLabs.Domain.Entities.Empadronamientos;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.Interfaces;
using EmpadronamientoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmpadronamientosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IS3Service _s3Service;
    private readonly IRekognitionService _rekognitionService;
    private readonly ILogger<EmpadronamientosController> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _config;

    public EmpadronamientosController(
        ApplicationDbContext context,
        IS3Service s3Service,
        IRekognitionService rekognitionService,
        ILogger<EmpadronamientosController> logger,
        ICurrentUserService currentUser,
        IConfiguration config)
    {
        _context = context;
        _s3Service = s3Service;
        _rekognitionService = rekognitionService;
        _logger = logger;
        _currentUser = currentUser;
        _config = config;
    }

    [HttpPost("validar")]
    [AuthLvl("e", 2)]
    [EndpointSummary("Paso 1: Validar estructura del JSON de empadronamiento")]
    public IActionResult Validar([FromBody] CrearEmpadronamientoRequest request)
    {
        if (!ModelState.IsValid)
            return Error("Estructura de datos inválida");

        return Result(new { validado = true }, "Estructura correcta");
    }

    [HttpPost("subir-foto")]
    [AuthLvl("e", 2)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Paso 2: Subir foto a S3 y validar rostro")]
    public async Task<IActionResult> SubirFoto(IFormFile archivo, [FromForm] string tipo)
    {
        if (archivo == null || archivo.Length == 0)
            return Error("No se proporcionó un archivo válido.");

        int organizacionId = _currentUser.OrganizacionId;

        if (tipo.Equals("Cara", StringComparison.OrdinalIgnoreCase))
        {
            var validation = await _rekognitionService.ValidateFaceAsync(archivo.OpenReadStream());
            if (!validation.IsValid) return Error(validation.ErrorMessage);
        }

        var s3Key = $"Fotos/{organizacionId}/{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        await _s3Service.UploadImageAsync(archivo.OpenReadStream(), s3Key, archivo.ContentType);

        var foto = new Foto
        {
            OrganizacionId = organizacionId,
            S3Key = s3Key,
            Tipo = tipo,
            Descripcion = $"Subida individual: {tipo}"
        };

        _context.Fotos.Add(foto);
        await _context.SaveChangesAsync();

        return Result(new { id = foto.Id, nombre = archivo.FileName, tipo = foto.Tipo }, "Foto procesada");
    }

    [HttpPost("completar")]
[AuthLvl("e", 2)]
[EndpointSummary("Guardado Integral: Registro de lugar, empadronamiento con folio, personas y detalles biométricos")]
public async Task<IActionResult> Completar([FromBody] CrearEmpadronamientoRequest request)
{
    int orgId = _currentUser.OrganizacionId;
    int usuarioId = int.TryParse(_currentUser.UserId, out var id) ? id : 0;

    // 1. PRE-VALIDACIÓN DE FOTOS (Incluyendo la foto del LUGAR)
    var fotoIdsEnRequest = request.Personas
        .SelectMany(p => p.Fotos.Select(f => f.FotoId))
        .Concat(request.Personas.Where(p => p.Rostro != null).Select(p => p.Rostro!.FotoId))
        .ToList();

    // Agregamos el ID de la foto del lugar para que entre en el diccionario
    if (request.Lugar.ImagenID > 0) fotoIdsEnRequest.Add(request.Lugar.ImagenID);

    fotoIdsEnRequest = fotoIdsEnRequest.Distinct().ToList();

    var diccionarioFotos = await _context.Fotos
        .Where(f => fotoIdsEnRequest.Contains(f.Id) && f.OrganizacionId == orgId)
        .ToDictionaryAsync(f => f.Id);

    if (diccionarioFotos.Count != fotoIdsEnRequest.Count)
    {
        var invalidas = fotoIdsEnRequest.Where(id => !diccionarioFotos.ContainsKey(id));
        return Error($"IDs de fotos inválidos o sin acceso: {string.Join(", ", invalidas)}", "INVALID_PHOTOS");
    }

    // LEER CONFIGURACIÓN DE AWS (Appsettings.json)
    var bucketName = _config["AWS:S3:Buckets:Empadronamiento"];
    var collectionId = _config["AWS:Rekognition:Collections:Empadronamiento"];

    // 2. MANEJO DE ESTRATEGIA DE EJECUCIÓN
    var strategy = _context.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // A. GUARDAR LUGAR DEL EVENTO
            var lugar = new LugarEmpadronamiento
            {
                Calle = request.Lugar.Calle,
                NumeroExterior = request.Lugar.NumeroExterior,
                NumeroInterior = request.Lugar.NumeroInterior,
                CP = request.Lugar.CP,
                Colonia = request.Lugar.Colonia,
                Municipio = request.Lugar.Municipio,
                Estado = request.Lugar.Estado,
                Referencia = request.Lugar.Referencia,
                Latitud = request.Lugar.Latitud,
                Longitud = request.Lugar.Longitud,
                ImagenId = request.Lugar.ImagenID,
                OrganizacionId = orgId
            };
            _context.LugaresEmpadronamiento.Add(lugar);
            await _context.SaveChangesAsync();

            // B. GUARDAR CABECERA DE EMPADRONAMIENTO
            var empadronamiento = new Empadronamiento
            {
                Fecha = request.Fecha,
                Hora = request.Hora,
                CRP = request.CRPN,
                NarrativaHechos = request.NarrativaHechos,
                UsuarioResponsableId = usuarioId,
                LugarEmpadronamientoId = lugar.Id,
                OrganizacionId = orgId
            };
            _context.Empadronamientos.Add(empadronamiento);
            await _context.SaveChangesAsync();

            // GENERACIÓN DE FOLIO ESCALABLE
            empadronamiento.Folio = $"EMP-O{orgId}-{DateTime.Now.Year}-{empadronamiento.Id:D7}";
            _context.Empadronamientos.Update(empadronamiento);
            
            // --- ACTUALIZAR FOTO DEL LUGAR ---
            if (request.Lugar.ImagenID > 0 && diccionarioFotos.TryGetValue(request.Lugar.ImagenID, out var fotoLugar))
            {
                fotoLugar.IdEmpadronamiento = empadronamiento.Id;
                fotoLugar.OrganizacionId = orgId;
                fotoLugar.Tipo = "Lugar";
                fotoLugar.Descripcion = $"Foto del lugar del evento: {lugar.Calle}";
                _context.Fotos.Update(fotoLugar);
            }

            await _context.SaveChangesAsync();

            // C. PROCESAR PERSONAS Y DETALLES ANIDADOS
            foreach (var pDto in request.Personas)
            {
                var persona = new Persona
                {
                    Nombre = pDto.Nombre,
                    ApellidoPaterno = pDto.ApellidoPaterno,
                    ApellidoMaterno = pDto.ApellidoMaterno,
                    FechaNacimiento = pDto.FechaNacimiento,
                    Edad = pDto.Edad,
                    Estatura = pDto.Estatura,
                    Sexo = pDto.Sexo,
                    Originario = pDto.Originario,
                    Telefono = pDto.Telefono,
                    Apodo = pDto.Apodo,
                    Nacionalidad = pDto.Nacionalidad,
                    EstadoCivil = pDto.EstadoCivil,
                    Escolaridad = pDto.Escolaridad,
                    OficioProfesion = pDto.OficioProfesion,
                    ObservacionesGenerales = pDto.ObservacionesGenerales,
                    OrganizacionId = orgId
                };

                // 1. DIRECCIÓN DE LA PERSONA
                persona.Direcciones.Add(new DireccionPersona
                {
                    Calle = pDto.Direccion.Calle,
                    NumeroExterior = pDto.Direccion.NumeroExterior,
                    NumeroInterior = pDto.Direccion.NumeroInterior,
                    CP = pDto.Direccion.CP,
                    Colonia = pDto.Direccion.Colonia,
                    Municipio = pDto.Direccion.Municipio,
                    Estado = pDto.Direccion.Estado,
                    Pais = pDto.Direccion.Pais,
                    Referencia = pDto.Direccion.Referencia,
                    Latitud = pDto.Direccion.Latitud,
                    Longitud = pDto.Direccion.Longitud,
                    EsPrincipal = pDto.Direccion.EsPrincipal,
                    OrganizacionId = orgId
                });

                // 2. FAMILIARES
                if (pDto.Familiares != null)
                {
                    foreach (var f in pDto.Familiares)
                    {
                        persona.Familiares.Add(new Familiar {
                            NombreCompleto = f.NombreCompleto, Parentesco = f.Parentesco,
                            Telefono = f.Telefono, Direccion = f.Direccion, OrganizacionId = orgId
                        });
                    }
                }

                // 3. REDES SOCIALES
                if (pDto.RedesSociales != null)
                {
                    foreach (var rs in pDto.RedesSociales)
                    {
                        persona.RedesSociales.Add(new RedSocial {
                            TipoRedSocial = rs.TipoRedSocial, Usuario = rs.Usuario,
                            UrlPerfil = rs.UrlPerfil, OrganizacionId = orgId
                        });
                    }
                }

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                // 4. RELACIÓN INTERMEDIA
                _context.EmpadronamientoPersonas.Add(new EmpadronamientoPersona {
                    EmpadronamientoId = empadronamiento.Id,
                    PersonaId = persona.Id,
                    Observaciones = pDto.ObservacionesEmpadronamiento,
                    OrganizacionId = orgId
                });

                // 5. ACTUALIZAR FOTOS DE LA PERSONA
                foreach (var fDto in pDto.Fotos)
                {
                    var foto = diccionarioFotos[fDto.FotoId];
                    foto.IdPersona = persona.Id;
                    foto.IdEmpadronamiento = empadronamiento.Id;
                    foto.OrganizacionId = orgId;
                    foto.Tipo = fDto.TipoFoto;
                    foto.Descripcion = fDto.Descripcion;
                    _context.Fotos.Update(foto);
                }

                // 6. ROSTRO Y AWS REKOGNITION
                if (pDto.Rostro != null)
                {
                    var fotoRostro = diccionarioFotos[pDto.Rostro.FotoId];
                    fotoRostro.IdPersona = persona.Id;
                    fotoRostro.IdEmpadronamiento = empadronamiento.Id;
                    fotoRostro.OrganizacionId = orgId;
                    _context.Fotos.Update(fotoRostro);

                    var resRek = await _rekognitionService.IndexFaceAndGetDetailsAsync(
                        bucketName!, fotoRostro.S3Key, collectionId!, $"persona_{persona.Id}");
                    
                    if (resRek.Exito)
                    {
                        _context.Caras.Add(new Cara {
                            IdFoto = fotoRostro.Id, OrganizacionId = orgId,
                            FaceId = resRek.FaceId!, S3Key = fotoRostro.S3Key,
                            BoundingBox = resRek.BoundingBoxJson, Confidence = resRek.Confidence ?? 0
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result(new { id = empadronamiento.Id, folio = empadronamiento.Folio }, "Registro guardado íntegramente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fallo crítico en el guardado de empadronamiento");
            var msg = ex.InnerException?.Message ?? ex.Message;
            return Error($"Error de persistencia: {msg}", "SAVE_FULL_ERROR");
        }
    });
}
}