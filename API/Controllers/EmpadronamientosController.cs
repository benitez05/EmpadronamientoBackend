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
        return Result(new
        {
            id = foto.Id,
            nombre = archivo.FileName,
            tipo = foto.Tipo,
            url = _s3Service.GetPreSignedUrl(keyFinal)
        }, "Foto procesada");
    }

    /// <summary>
    /// Finaliza el proceso de empadronamiento vinculando toda la información recopilada.
    /// 
    /// Operaciones principales:
    /// - Evento: Crea un nuevo registro de Empadronamiento y su Lugar asociado, generando un folio único.
    /// - Personas (Upsert): Si el objeto recibe un ID, actualiza los datos generales de la persona existente. Si no, crea una nueva.
    /// - Entidades hijas (Upsert): Actualiza o crea Direcciones, Redes Sociales y Familiares dependiendo de si incluyen su ID correspondiente.
    /// - Fotografías: Asigna las fotos subidas previamente al evento y a la persona.
    /// - Biometría: Valida estrictamente que el FotoId proporcionado para el rostro sea de tipo 'Biométrico Facial', verifica que no haya sido indexado previamente, y lo registra en la colección de AWS Rekognition para búsquedas futuras.
    /// </summary>
    [HttpPost("completar")]
    [AuthLvl("e", 2)]
    [EndpointSummary("Paso 3: Guardado integral de empadronamiento (Creación de evento, Upsert de personas e indexación biométrica)")]
    public async Task<IActionResult> Completar([FromBody] CrearEmpadronamientoRequest request)
    {
        int orgId = _currentUser.OrganizacionId;
        int usuarioId = int.TryParse(_currentUser.UserId, out var id) ? id : 0;

        // 1. RECOPILACIÓN DE IDS DE FOTOS
        var fotoIdsEnRequest = request.Personas
            .SelectMany(p => p.Fotos.Select(f => f.FotoId))
            .Concat(request.Personas.Where(p => p.Rostro != null).Select(p => p.Rostro!.FotoId))
            .ToList();

        if (request.Lugar.ImagenID > 0) fotoIdsEnRequest.Add((int)request.Lugar.ImagenID); // Casteo preventivo

        fotoIdsEnRequest = fotoIdsEnRequest.Distinct().ToList();

        var diccionarioFotos = await _context.Fotos
            .Where(f => fotoIdsEnRequest.Contains(f.Id) && f.OrganizacionId == orgId)
            .ToDictionaryAsync(f => f.Id);

        // VALIDACIÓN DE EXISTENCIA
        if (diccionarioFotos.Count != fotoIdsEnRequest.Count)
        {
            var invalidas = fotoIdsEnRequest.Where(i => !diccionarioFotos.ContainsKey(i));
            return Error($"IDs de fotos inválidos o sin acceso: {string.Join(", ", invalidas)}", "INVALID_PHOTOS");
        }

        var bucketName = _config["AWS:S3:Buckets:Empadronamiento"];
        var collectionId = _config["AWS:Rekognition:Collections:Empadronamiento"];

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. GUARDAR LUGAR (El evento de empadronamiento es nuevo)
                var lugar = new LugarEmpadronamiento
                {
                    Calle = request.Lugar.Calle ?? "",
                    NumeroExterior = request.Lugar.NumeroExterior ?? "",
                    NumeroInterior = request.Lugar.NumeroInterior ?? "",
                    CP = request.Lugar.CP,
                    Colonia = request.Lugar.Colonia ?? "",
                    Municipio = request.Lugar.Municipio ?? "",
                    Estado = request.Lugar.Estado ?? "",
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
                    NarrativaHechos = request.NarrativaHechos ?? "",
                    UsuarioResponsableId = usuarioId,
                    LugarEmpadronamientoId = lugar.Id,
                    OrganizacionId = orgId
                };
                _context.Empadronamientos.Add(empadronamiento);
                await _context.SaveChangesAsync();

                empadronamiento.Folio = $"EMP-O{orgId}-{DateTime.Now.Year}-{empadronamiento.Id:D7}";
                _context.Empadronamientos.Update(empadronamiento);

                // ACTUALIZAR FOTO LUGAR
                if (request.Lugar.ImagenID > 0 && diccionarioFotos.TryGetValue((int)request.Lugar.ImagenID, out var fotoLugar))
                {
                    fotoLugar.IdEmpadronamiento = empadronamiento.Id;
                    fotoLugar.Tipo = "Lugar / Escena";
                    _context.Fotos.Update(fotoLugar);
                }

                // C. PROCESAR PERSONAS
                foreach (var pDto in request.Personas)
                {
                    Persona persona;
                    bool esPersonaNueva = false;

                    // VALIDAR SI VIENE ID PARA ACTUALIZAR O ES NUEVA
                    // Asumimos que pDto.Id es int? o int con valor por defecto 0
                    if (pDto.Id > 0)
                    {
                        persona = await _context.Personas
                            .Include(p => p.Direcciones)
                            .Include(p => p.RedesSociales)
                            .Include(p => p.Familiares)
                            .FirstOrDefaultAsync(p => p.Id == pDto.Id && p.OrganizacionId == orgId);

                        if (persona == null)
                        {
                            await transaction.RollbackAsync();
                            return Error($"La persona con ID {pDto.Id} no existe o no pertenece a la organización.", "PERSONA_NOT_FOUND");
                        }

                        // Actualización de datos principales (se reemplazan solo si envían un valor nuevo, o lo adaptas a tu regla de negocio)
                        persona.Nombre = pDto.Nombre ?? persona.Nombre;
                        persona.ApellidoPaterno = pDto.ApellidoPaterno ?? persona.ApellidoPaterno;
                        persona.ApellidoMaterno = pDto.ApellidoMaterno ?? persona.ApellidoMaterno;
                        persona.FechaNacimiento = pDto.FechaNacimiento;
                        persona.Edad = pDto.Edad ?? persona.Edad;
                        persona.Estatura = pDto.Estatura ?? persona.Estatura;
                        persona.Sexo = pDto.Sexo ?? persona.Sexo;
                        persona.Originario = pDto.Originario ?? persona.Originario;
                        persona.Telefono = pDto.Telefono ?? persona.Telefono;
                        persona.Apodo = pDto.Apodo ?? persona.Apodo;
                        persona.Nacionalidad = pDto.Nacionalidad ?? persona.Nacionalidad;
                        persona.EstadoCivil = pDto.EstadoCivil ?? persona.EstadoCivil;
                        persona.Escolaridad = pDto.Escolaridad ?? persona.Escolaridad;
                        persona.OficioProfesion = pDto.OficioProfesion ?? persona.OficioProfesion;
                        persona.ObservacionesGenerales = pDto.ObservacionesGenerales ?? persona.ObservacionesGenerales;

                        _context.Personas.Update(persona);
                    }
                    else
                    {
                        esPersonaNueva = true;
                        persona = new Persona
                        {
                            Nombre = pDto.Nombre ?? "",
                            ApellidoPaterno = pDto.ApellidoPaterno ?? "",
                            ApellidoMaterno = pDto.ApellidoMaterno ?? "",
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
                        _context.Personas.Add(persona);
                    }

                    // Forzar guardado para obtener el ID de la persona si es nueva y poder asignarlo a sus entidades hijas
                    await _context.SaveChangesAsync();

                    // 1. DIRECCIÓN
                    if (pDto.Direccion != null)
                    {
                        if (pDto.Direccion.Id > 0 && !esPersonaNueva)
                        {
                            var dirExistente = persona.Direcciones.FirstOrDefault(d => d.Id == pDto.Direccion.Id);
                            if (dirExistente != null)
                            {
                                dirExistente.Calle = pDto.Direccion.Calle ?? dirExistente.Calle;
                                dirExistente.NumeroExterior = pDto.Direccion.NumeroExterior ?? dirExistente.NumeroExterior;
                                dirExistente.NumeroInterior = pDto.Direccion.NumeroInterior ?? dirExistente.NumeroInterior;
                                dirExistente.CP = pDto.Direccion.CP > 0 ? pDto.Direccion.CP : dirExistente.CP;
                                dirExistente.Colonia = pDto.Direccion.Colonia ?? dirExistente.Colonia;
                                dirExistente.Municipio = pDto.Direccion.Municipio ?? dirExistente.Municipio;
                                dirExistente.Estado = pDto.Direccion.Estado ?? dirExistente.Estado;
                                dirExistente.Pais = pDto.Direccion.Pais ?? dirExistente.Pais;
                                dirExistente.Referencia = pDto.Direccion.Referencia ?? dirExistente.Referencia;
                                dirExistente.Latitud = pDto.Direccion.Latitud;
                                dirExistente.Longitud = pDto.Direccion.Longitud;
                                dirExistente.EsPrincipal = pDto.Direccion.EsPrincipal;
                            }
                        }
                        else
                        {
                            _context.DireccionesPersona.Add(new DireccionPersona
                            {
                                PersonaId = persona.Id,
                                Calle = pDto.Direccion.Calle ?? "",
                                NumeroExterior = pDto.Direccion.NumeroExterior ?? "",
                                NumeroInterior = pDto.Direccion.NumeroInterior ?? "",
                                CP = pDto.Direccion.CP,
                                Colonia = pDto.Direccion.Colonia ?? "",
                                Municipio = pDto.Direccion.Municipio ?? "",
                                Estado = pDto.Direccion.Estado ?? "",
                                Pais = pDto.Direccion.Pais ?? "México",
                                Referencia = pDto.Direccion.Referencia,
                                Latitud = pDto.Direccion.Latitud,
                                Longitud = pDto.Direccion.Longitud,
                                EsPrincipal = pDto.Direccion.EsPrincipal,
                                OrganizacionId = orgId
                            });
                        }
                    }

                    // 2. REDES SOCIALES
                    if (pDto.RedesSociales != null)
                    {
                        foreach (var rs in pDto.RedesSociales)
                        {
                            if (rs.Id > 0 && !esPersonaNueva)
                            {
                                var rsExistente = persona.RedesSociales.FirstOrDefault(r => r.Id == rs.Id);
                                if (rsExistente != null)
                                {
                                    rsExistente.TipoRedSocial = rs.TipoRedSocial ?? rsExistente.TipoRedSocial;
                                    rsExistente.Usuario = rs.Usuario ?? rsExistente.Usuario;
                                    rsExistente.UrlPerfil = rs.UrlPerfil ?? rsExistente.UrlPerfil;
                                }
                            }
                            else
                            {
                                _context.RedesSociales.Add(new RedSocial
                                {
                                    PersonaId = persona.Id,
                                    TipoRedSocial = rs.TipoRedSocial ?? "",
                                    Usuario = rs.Usuario ?? "",
                                    UrlPerfil = rs.UrlPerfil,
                                    OrganizacionId = orgId
                                });
                            }
                        }
                    }

                    // 3. FAMILIARES
                    if (pDto.Familiares != null)
                    {
                        foreach (var fam in pDto.Familiares)
                        {
                            if (fam.Id > 0 && !esPersonaNueva)
                            {
                                var famExistente = persona.Familiares.FirstOrDefault(f => f.Id == fam.Id);
                                if (famExistente != null)
                                {
                                    famExistente.NombreCompleto = fam.NombreCompleto ?? famExistente.NombreCompleto;
                                    famExistente.Parentesco = fam.Parentesco ?? famExistente.Parentesco;
                                    famExistente.Telefono = fam.Telefono ?? famExistente.Telefono;
                                    famExistente.Direccion = fam.Direccion ?? famExistente.Direccion;
                                }
                            }
                            else
                            {
                                _context.Familiares.Add(new Familiar
                                {
                                    PersonaId = persona.Id,
                                    NombreCompleto = fam.NombreCompleto ?? "",
                                    Parentesco = fam.Parentesco,
                                    Telefono = fam.Telefono,
                                    Direccion = fam.Direccion,
                                    OrganizacionId = orgId
                                });
                            }
                        }
                    }

                    // 4. RELACIÓN INTERMEDIA (El empadronamiento siempre es un evento nuevo para esa persona)
                    _context.EmpadronamientoPersonas.Add(new EmpadronamientoPersona
                    {
                        EmpadronamientoId = empadronamiento.Id,
                        PersonaId = persona.Id,
                        Observaciones = pDto.ObservacionesEmpadronamiento,
                        OrganizacionId = orgId
                    });

                    // 5. ACTUALIZAR FOTOS PERSONA
                    if (pDto.Fotos != null)
                    {
                        foreach (var fDto in pDto.Fotos)
                        {
                            var foto = diccionarioFotos[fDto.FotoId];
                            foto.IdPersona = persona.Id;
                            foto.IdEmpadronamiento = empadronamiento.Id;
                            foto.Tipo = fDto.TipoFoto;
                            foto.Descripcion = fDto.Descripcion;
                            _context.Fotos.Update(foto);
                        }
                    }

                    // 6. ROSTRO Y REKOGNITION
                    if (pDto.Rostro != null)
                    {
                        var fotoRostro = diccionarioFotos[pDto.Rostro.FotoId];

                        // 1. Validar que el ID que mandaron REALMENTE corresponda a una foto biométrica
                        // Esto garantiza que la imagen ya pasó por tu ValidateFaceAsync en el endpoint de subida.
                        if (string.IsNullOrEmpty(fotoRostro.Tipo) ||
                            !fotoRostro.Tipo.Equals("Biométrico Facial", StringComparison.OrdinalIgnoreCase))
                        {
                            await transaction.RollbackAsync();
                            return Error($"La foto con ID {fotoRostro.Id} no está clasificada como 'Biométrico Facial'.", "INVALID_PHOTO_TYPE");
                        }

                        // 2. Defensa estricta: La foto siempre debe ser nueva.
                        bool fotoYaUsada = await _context.Caras.AnyAsync(c => c.IdFoto == fotoRostro.Id);
                        if (fotoYaUsada)
                        {
                            await transaction.RollbackAsync();
                            return Error($"El ID de foto {fotoRostro.Id} ya fue indexado previamente. Se requiere una captura nueva.", "REUSED_FOTO_ID");
                        }

                        // 3. Indexar la fotografía en AWS Rekognition
                        var resRek = await _rekognitionService.IndexFaceAndGetDetailsAsync(
                            bucketName!, fotoRostro.S3Key, collectionId!, $"persona_{persona.Id}");

                        if (!resRek.Exito)
                        {
                            await transaction.RollbackAsync();
                            return Error("AWS Rekognition no pudo procesar el rostro en la imagen.", "REKOGNITION_ERROR");
                        }

                        // 4. Guardar el registro biométrico
                        _context.Caras.Add(new Cara
                        {
                            IdFoto = fotoRostro.Id,
                            FaceId = resRek.FaceId!,
                            S3Key = fotoRostro.S3Key,
                            BoundingBox = resRek.BoundingBoxJson,
                            Confidence = resRek.Confidence ?? 0
                        });

                        // 5. Actualizar la entidad Foto con sus relaciones
                        fotoRostro.IdPersona = persona.Id;
                        fotoRostro.IdEmpadronamiento = empadronamiento.Id;
                        _context.Fotos.Update(fotoRostro);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result(new { id = empadronamiento.Id, folio = empadronamiento.Folio }, "Registro guardado e integrado exitosamente.");
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