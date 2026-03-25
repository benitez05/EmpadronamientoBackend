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

    private static readonly List<string> TiposValidos = new()
    {
        "Biométrico Facial", "Lugar / Escena", "Identificación (Frente)",
        "Identificación (Reverso)", "Perfil Izquierdo", "Perfil Derecho",
        "Tatuaje / Seña", "Vehículo", "Pertenencias", "Cuerpo Completo"
    };

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
    [EndpointSummary("Paso 2: Subir foto a S3 y validar catálogo")]
    public async Task<IActionResult> SubirFoto(IFormFile archivo, [FromForm] string tipo)
    {
        if (archivo == null || archivo.Length == 0)
            return Error("No se proporcionó un archivo válido.");

        if (!TiposValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase))
        {
            return Error($"El tipo de foto '{tipo}' no es válido. Usa: {string.Join(", ", TiposValidos)}", "INVALID_PHOTO_TYPE");
        }

        int organizacionId = _currentUser.OrganizacionId;

        if (tipo.Equals("Biométrico Facial", StringComparison.OrdinalIgnoreCase))
        {
            var validation = await _rekognitionService.ValidateFaceAsync(archivo.OpenReadStream());
            if (!validation.IsValid) return Error(validation.ErrorMessage);
        }

        // --- NUEVA LÓGICA DE PREFIJOS ---
        // Definimos la subcarpeta funcional
        var pathSugerido = $"Fotos/{organizacionId}/{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        
        // El servicio inyecta dev/ o prod/ y nos regresa la key completa con prefijo
        var keyFinal = await _s3Service.UploadImageAsync(archivo.OpenReadStream(), pathSugerido, archivo.ContentType);

        var foto = new Foto
        {
            OrganizacionId = organizacionId,
            S3Key = keyFinal, // Guardamos la ruta real (ej: dev/Fotos/...)
            Tipo = tipo,
            Descripcion = $"Subida individual: {tipo}"
        };

        _context.Fotos.Add(foto);
        await _context.SaveChangesAsync();

        // Para la respuesta, generamos una URL temporal (Pre-firmada) como pediste
        return Result(new { 
            id = foto.Id, 
            nombre = archivo.FileName, 
            tipo = foto.Tipo,
            url = _s3Service.GetPreSignedUrl(keyFinal) 
        }, "Foto procesada");
    }

   [HttpPost("completar")]
[AuthLvl("e", 2)]
[EndpointSummary("Paso 3 : Guardado Integral")]
public async Task<IActionResult> Completar([FromBody] CrearEmpadronamientoRequest request)
{
    int orgId = _currentUser.OrganizacionId;
    int usuarioId = int.TryParse(_currentUser.UserId, out var id) ? id : 0;

    // 1. RECOPILACIÓN DE IDS DE FOTOS
    var fotoIdsEnRequest = request.Personas
        .SelectMany(p => p.Fotos.Select(f => f.FotoId))
        .Concat(request.Personas.Where(p => p.Rostro != null).Select(p => p.Rostro!.FotoId))
        .ToList();

    if (request.Lugar.ImagenID > 0) fotoIdsEnRequest.Add(request.Lugar.ImagenID);

    fotoIdsEnRequest = fotoIdsEnRequest.Distinct().ToList();

    var diccionarioFotos = await _context.Fotos
        .Where(f => fotoIdsEnRequest.Contains(f.Id) && f.OrganizacionId == orgId)
        .ToDictionaryAsync(f => f.Id);

    // VALIDACIÓN DE EXISTENCIA
    if (diccionarioFotos.Count != fotoIdsEnRequest.Count)
    {
        var invalidas = fotoIdsEnRequest.Where(id => !diccionarioFotos.ContainsKey(id));
        return Error($"IDs de fotos inválidos o sin acceso: {string.Join(", ", invalidas)}", "INVALID_PHOTOS");
    }

    // --- VALIDACIONES DE CATÁLOGO (NO TOCAR) ---
    var tiposValidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Biométrico Facial", "Lugar / Escena", "Identificación (Frente)",
        "Identificación (Reverso)", "Perfil Izquierdo", "Perfil Derecho",
        "Tatuaje / Seña", "Vehículo", "Pertenencias", "Cuerpo Completo"
    };

    if (request.Lugar.ImagenID > 0 && diccionarioFotos.TryGetValue(request.Lugar.ImagenID, out var dbFotoLugar))
    {
        if (!dbFotoLugar.Tipo.Equals("Lugar / Escena", StringComparison.OrdinalIgnoreCase))
            return Error("La foto asignada al lugar no es válida.", "WRONG_PHOTO_TYPE");
    }

    var bucketName = _config["AWS:S3:Buckets:Empadronamiento"];
    var collectionId = _config["AWS:Rekognition:Collections:Empadronamiento"];

    var strategy = _context.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // A. GUARDAR LUGAR
            var lugar = new LugarEmpadronamiento
            {
                Calle = request.Lugar.Calle,
                NumeroExterior = request.Lugar.NumeroExterior,
                NumeroInterior = request.Lugar.NumeroInterior ?? "",
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

            // B. GUARDAR CABECERA
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

            empadronamiento.Folio = $"EMP-O{orgId}-{DateTime.Now.Year}-{empadronamiento.Id:D7}";
            _context.Empadronamientos.Update(empadronamiento);

            // ACTUALIZAR FOTO LUGAR
            if (request.Lugar.ImagenID > 0 && diccionarioFotos.TryGetValue(request.Lugar.ImagenID, out var fotoLugar))
            {
                fotoLugar.IdEmpadronamiento = empadronamiento.Id;
                fotoLugar.Tipo = "Lugar / Escena";
                _context.Fotos.Update(fotoLugar);
            }

            // C. PROCESAR PERSONAS
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

                // --- MAPPEO DE HIJOS (LO QUE FALTABA) ---
                
                // 1. Dirección
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

                // 2. Redes Sociales
                foreach (var rs in pDto.RedesSociales)
                {
                    persona.RedesSociales.Add(new RedSocial
                    {
                        TipoRedSocial = rs.TipoRedSocial,
                        Usuario = rs.Usuario,
                        UrlPerfil = rs.UrlPerfil,
                        OrganizacionId = orgId
                    });
                }

                // 3. Familiares
                foreach (var fam in pDto.Familiares)
                {
                    persona.Familiares.Add(new Familiar
                    {
                        NombreCompleto = fam.NombreCompleto,
                        Parentesco = fam.Parentesco,
                        Telefono = fam.Telefono,
                        Direccion = fam.Direccion,
                        OrganizacionId = orgId
                    });
                }

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                // 4. RELACIÓN INTERMEDIA
                _context.EmpadronamientoPersonas.Add(new EmpadronamientoPersona
                {
                    EmpadronamientoId = empadronamiento.Id,
                    PersonaId = persona.Id,
                    Observaciones = pDto.ObservacionesEmpadronamiento,
                    OrganizacionId = orgId
                });

                // 5. ACTUALIZAR FOTOS PERSONA
                foreach (var fDto in pDto.Fotos)
                {
                    var foto = diccionarioFotos[fDto.FotoId];
                    foto.IdPersona = persona.Id;
                    foto.IdEmpadronamiento = empadronamiento.Id;
                    foto.Tipo = fDto.TipoFoto;
                    foto.Descripcion = fDto.Descripcion;
                    _context.Fotos.Update(foto);
                }

                // 6. ROSTRO Y REKOGNITION (LO TENEMOS DE VUELTA)
                if (pDto.Rostro != null)
                {
                    var fotoRostro = diccionarioFotos[pDto.Rostro.FotoId];
                    fotoRostro.IdPersona = persona.Id;
                    fotoRostro.IdEmpadronamiento = empadronamiento.Id;
                    fotoRostro.Tipo = "Biométrico Facial";
                    _context.Fotos.Update(fotoRostro);

                    var resRek = await _rekognitionService.IndexFaceAndGetDetailsAsync(
                        bucketName!, fotoRostro.S3Key, collectionId!, $"persona_{persona.Id}");

                    if (resRek.Exito)
                    {
                        _context.Caras.Add(new Cara
                        {
                            IdFoto = fotoRostro.Id,
                            OrganizacionId = orgId,
                            FaceId = resRek.FaceId!,
                            S3Key = fotoRostro.S3Key,
                            BoundingBox = resRek.BoundingBoxJson,
                            Confidence = resRek.Confidence ?? 0
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
            _logger.LogError(ex, "Fallo crítico en guardado");
            return Error($"Error: {ex.Message}");
        }
    });
}
}