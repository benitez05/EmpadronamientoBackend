using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmpadronamientoBackend.Application.DTOs.Requests;
using EmpadronamientoBackend.Application.DTOs.Responses;
using EmpadronamientoBackend.Application.Mappers;
using EmpadronamientoBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using EmpadronamientoBackend.Infrastructure.Persistence;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Infrastructure.Services;
using BenitezLabs.API.Authorization;
using EmpadronamientoBackend.Application.Utils;

namespace EmpadronamientoBackend.API.Controllers;

// Asegúrate de tener este using arriba:
// using EmpadronamientoBackend.Application.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly ITwoFactorService _twoFactorService;

    public AuthController(
        ApplicationDbContext context,
        IPasswordService passwordService,
        ICacheService cacheService,
        ICurrentUserService currentUser,
        IEmailService emailService,
        ITwoFactorService twoFactorService)
    {
        _context = context;
        _passwordService = passwordService;
        _cacheService = cacheService;
        _currentUser = currentUser;
        _emailService = emailService;
        _twoFactorService = twoFactorService;
    }

    // ... (Aquí siguen tus regiones actuales) ...

    #region REGISTRO Y LOGIN

    [AuthLvl("u", 2)]
    [HttpPost("registrar-usuario")]
    [EndpointSummary("Registro de usuarios ")]
    public async Task<IActionResult> RegisterStaff([FromBody] RegisterRequest request)
    {
        // 1. Verificación de duplicados
        if (await _context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Correo == request.Correo))
            return Error("Ya existe una cuenta asociada a este correo electrónico.");

        var orgId = _currentUser.OrganizacionId;
        if (orgId == 0) return Error("No se pudo determinar tu organización.");

        // 2. VALIDACIÓN DEL ROL: ¿Existe y pertenece a mi empresa?
        var rolExiste = await _context.Roles
            .AnyAsync(r => r.Id == request.RoleId && r.OrganizacionId == orgId);

        if (!rolExiste)
            return Error("El rol seleccionado no es válido.");

        // 3. Creación
        var user = request.ToEntity();
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);
        user.OrganizacionId = orgId;
        user.RoleId = request.RoleId; // Usamos el ID del request
        user.Tipo = 1;

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        return Result(user.ToResponse(), "Usuario registrado exitosamente.");
    }

    [AuthLvl("u", 3)] // Solo nivel 3 o superior (Staff/Dev)
    [HttpPost("register-external-admin")]
    [EndpointSummary("Registro de administradores externos")]
    public async Task<IActionResult> RegisterExternalAdmin([FromBody] RegisterExternalAdminRequest request)
    {
        // EL CANDADO: Solo niveles 3 y 4 (Staff de BenitezLabs) pueden crear otros admins
        if (_currentUser.Tipo < 3)
        {
            return Error("No tienes nivel suficiente para registrar administradores externos.");
        }

        // 1. Validar Configuración Global Dinámica
        var config = await _context.ConfiguracionesGlobales.AsNoTracking().FirstOrDefaultAsync();
        if (config == null) return Error("Configuración global no encontrada.");

        // Validar privilegio de Organización Maestra
        bool esMaestra = _currentUser.OrganizacionId == config.OrganizacionMaestraId;
        if (!esMaestra)
        {
            return Error("Acceso denegado: tu organización no tiene privilegios de administración global.");
        }

        // 2. Validar que la organización destino exista
        if (!await _context.Organizaciones.AnyAsync(o => o.Id == request.TargetOrganizacionId))
        {
            return Error("La organización destino no existe.");
        }

        // --- LA NUEVA VALIDACIÓN CRUCIAL ---
        // 3. Validar que el ROL pertenezca a la organización destino
        var rolValido = await _context.Roles
    .IgnoreQueryFilters()
    .AnyAsync(r => r.Id == request.RoleId && r.OrganizacionId == request.TargetOrganizacionId);

        if (!rolValido)
        {
            return Error($"El Rol ID {request.RoleId} no pertenece a la organización destino ({request.TargetOrganizacionId}).");
        }

        // 4. Verificación global de duplicados
        if (await _context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Correo == request.Correo))
        {
            return Error("El correo ya está registrado en el sistema.");
        }

        // 5. Creación del Admin Externo (Tipo 2)
        var user = request.ToEntity();
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);
        user.OrganizacionId = request.TargetOrganizacionId;
        user.RoleId = request.RoleId; // ID validado dinámicamente
        user.Tipo = 2; // Administrador de su propia empresa

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        return Result(user.ToResponse(), $"Administrador creado exitosamente para la organización {request.TargetOrganizacionId}.");
    }

    [HttpPost("login")]
    [EndpointSummary("Inicio de sesión")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Buscamos el usuario con toda la jerarquía
        var user = await _context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Organizacion)
                .ThenInclude(o => o.ModulosContratados)
                    .ThenInclude(om => om.Modulo)
            .Include(u => u.Role)
                .ThenInclude(r => r.Permisos)
                    .ThenInclude(p => p.Modulo)
            .SingleOrDefaultAsync(u => u.Correo == request.Correo);

        // 2. Validamos existencia del usuario
        if (user == null)
            return Error("El correo o la contraseña son incorrectos.");

        // 3. Verificamos si el usuario está bloqueado por intentos fallidos
        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.UtcNow)
            return Error($"Tu cuenta está bloqueada hasta {user.BloqueadoHasta.Value.ToLocalTime():HH:mm} por intentos fallidos.");

        // 4. Validamos contraseña
        if (!_passwordService.IsValidPassword(user, request.Password))
        {
            user.IntentosFallidos++;

            // Bloqueo tras 5 intentos fallidos
            if (user.IntentosFallidos >= 5)
                user.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();
            return Error("El correo o la contraseña son incorrectos.");
        }

        // 5. Resetear intentos fallidos tras login exitoso
        user.IntentosFallidos = 0;
        user.BloqueadoHasta = null;

        // 6. Validamos si el usuario está activo
        if (!user.Activo)
            return Error("Tu cuenta de usuario está desactivada.");

        // 7. Validaciones de Organización (Bypass Tipo 4)
        if (user.Tipo != 4)
        {
            if (user.Organizacion == null || !user.Organizacion.Activa)
                return Error("Tu organización está desactivada o suspendida.");

            if (user.Organizacion.FechaVencimiento < DateTime.UtcNow)
                return Error("El contrato de tu organización ha vencido.");
        }

        // 🚨 AQUÍ ENTRA LA MAGIA DEL 2FA 🚨
        if (user.DosPasosHabilitado)
        {
            await _context.SaveChangesAsync(); // Guardamos el reseteo de intentos fallidos

            // Generamos el ticket temporal para el caché (No es un JWT, es solo un ID seguro)
            var ticket2FA = Guid.NewGuid().ToString();

            // Guardamos en caché el ID del usuario ligado a este ticket por 5 minutos
            await _cacheService.SetAsync($"2FA_{ticket2FA}", user.Id, TimeSpan.FromMinutes(5));

            // Retornamos la bandera Requires2FA para que el Frontend muestre la pantalla del PIN
            return Result(new
            {
                Requires2FA = true,
                TempToken = ticket2FA
            }, "Credenciales correctas. Se requiere código de verificación 2FA.");
        }

        // 8. Lógica de permisos según tipo de usuario
        Dictionary<string, int> permisosDict;
        var modulosContratados = user.Organizacion?.ModulosContratados?
            .Where(om => om.Activo && om.Modulo != null)
            .Select(om => om.Modulo)
            .ToList() ?? new List<Modulo>();

        if (user.Tipo >= 2)
        {
            var modulosVisibles = user.Tipo == 4
                ? await _context.Modulos.ToListAsync()
                : modulosContratados;

            permisosDict = modulosVisibles.ToDictionary(m => m.K, _ => 3);
        }
        else
        {
            var contratadosIds = modulosContratados.Select(m => m.Id).ToList();
            permisosDict = user.Role?.Permisos?
                .Where(p => p.Modulo != null && contratadosIds.Contains(p.ModuloId))
                .ToDictionary(p => p.Modulo.K, p => p.Lvl)
                ?? new Dictionary<string, int>();
        }

        // 9. Generación de tokens y sesión
        var tokenData = _passwordService.GenerateJwtToken(user);
        var refreshToken = _passwordService.GenerateRefreshToken();

        var nuevaSesion = new UsuarioSesion
        {
            UsuarioId = user.Id,
            OrganizacionId = user.OrganizacionId,
            Jti = tokenData.Jti,
            RefreshToken = refreshToken,
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            FechaExpiracion = DateTime.UtcNow.AddDays(30)
        };

        _context.UsuarioSesiones.Add(nuevaSesion);
        user.UltimoLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result(new LoginResponse
        {
            Token = tokenData.Token,
            RefreshToken = refreshToken,
            Usuario = user.ToResponse(),
            Permisos = permisosDict
        }, "Ha iniciado sesión correctamente.");
    }

    [HttpPost("login-2fa")]
    [EndpointSummary("Valida el código 2FA y completa el inicio de sesión")]
    public async Task<IActionResult> Login2FA([FromBody] Login2FARequest request)
    {
        // 1. Buscamos el ticket en el caché
        var userId = await _cacheService.GetAsync<int?>($"2FA_{request.TempToken}");

        if (userId == null)
            return Error("El tiempo para ingresar el código expiró o el ticket es inválido. Por favor, inicia sesión nuevamente.");

        // 2. Traemos al usuario con toda su jerarquía (igual que en el Login normal)
        var user = await _context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.Organizacion)
                .ThenInclude(o => o.ModulosContratados)
                    .ThenInclude(om => om.Modulo)
            .Include(u => u.Role)
                .ThenInclude(r => r.Permisos)
                    .ThenInclude(p => p.Modulo)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null) return Error("Usuario no encontrado.");

        // 3. Validamos el código de 6 dígitos con nuestro servicio
        bool esValido = _twoFactorService.ValidateCode(user.DosPasosSecretKey, request.Codigo);

        if (!esValido)
            return Error("El código de verificación es incorrecto.");

        // 4. ¡ÉXITO! Destruimos el ticket del caché inmediatamente por seguridad
        await _cacheService.RemoveAsync($"2FA_{request.TempToken}");

        // 5. Calculamos los permisos (Misma lógica que el Login normal)
        Dictionary<string, int> permisosDict;
        var modulosContratados = user.Organizacion?.ModulosContratados?
            .Where(om => om.Activo && om.Modulo != null).Select(om => om.Modulo).ToList() ?? new List<Modulo>();

        if (user.Tipo >= 2)
        {
            var modulosVisibles = user.Tipo == 4 ? await _context.Modulos.ToListAsync() : modulosContratados;
            permisosDict = modulosVisibles.ToDictionary(m => m.K, _ => 3);
        }
        else
        {
            var contratadosIds = modulosContratados.Select(m => m.Id).ToList();
            permisosDict = user.Role?.Permisos?
                .Where(p => p.Modulo != null && contratadosIds.Contains(p.ModuloId))
                .ToDictionary(p => p.Modulo.K, p => p.Lvl) ?? new Dictionary<string, int>();
        }

        // 6. Generamos los tokens definitivos y guardamos la sesión
        var tokenData = _passwordService.GenerateJwtToken(user);
        var refreshToken = _passwordService.GenerateRefreshToken();

        var nuevaSesion = new UsuarioSesion
        {
            UsuarioId = user.Id,
            OrganizacionId = user.OrganizacionId,
            Jti = tokenData.Jti,
            RefreshToken = refreshToken,
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            FechaExpiracion = DateTime.UtcNow.AddDays(30)
        };

        _context.UsuarioSesiones.Add(nuevaSesion);
        user.UltimoLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result(new LoginResponse
        {
            Token = tokenData.Token,
            RefreshToken = refreshToken,
            Usuario = user.ToResponse(),
            Permisos = permisosDict
        }, "Ha iniciado sesión correctamente mediante autenticación de dos pasos.");
    }

    [HttpPost("refresh-token")]
    [EndpointSummary("Renovar Access Token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var sesion = await _context.UsuarioSesiones
            .IgnoreQueryFilters()
            .Include(s => s.Usuario)
                .ThenInclude(u => u.Organizacion)
                    .ThenInclude(o => o.ModulosContratados)
                        .ThenInclude(om => om.Modulo)
            .Include(s => s.Usuario)
                .ThenInclude(u => u.Role)
                    .ThenInclude(r => r.Permisos)
                        .ThenInclude(p => p.Modulo)
            .SingleOrDefaultAsync(s => s.RefreshToken == request.RefreshToken);

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
            return Error("La sesión ha expirado o el token es inválido.");

        var user = sesion.Usuario;
        if (!user.Activo) return Error("Cuenta desactivada.");

        if (user.Tipo != 4)
        {
            if (user.Organizacion == null || !user.Organizacion.Activa || user.Organizacion.FechaVencimiento < DateTime.UtcNow)
                return Error("Acceso denegado por estado de la organización.");
        }

        // RE-CALCULAR PERMISOS (Misma lógica que el Login)
        Dictionary<string, int> permisosDict;
        var modulosContratados = user.Organizacion?.ModulosContratados?
            .Where(om => om.Activo && om.Modulo != null)
            .Select(om => om.Modulo)
            .ToList() ?? new List<Modulo>();

        if (user.Tipo >= 2)
        {
            var modulosVisibles = user.Tipo == 4
                ? await _context.Modulos.ToListAsync()
                : modulosContratados;
            permisosDict = modulosVisibles.ToDictionary(m => m.K, _ => 3);
        }
        else
        {
            var contratadosIds = modulosContratados.Select(m => m.Id).ToList();
            permisosDict = user.Role?.Permisos?
                .Where(p => p.Modulo != null && contratadosIds.Contains(p.ModuloId))
                .ToDictionary(p => p.Modulo.K, p => p.Lvl)
                ?? new Dictionary<string, int>();
        }

        var tokenData = _passwordService.GenerateJwtToken(user);
        var newRefreshToken = _passwordService.GenerateRefreshToken();

        sesion.RefreshToken = newRefreshToken;
        sesion.Jti = tokenData.Jti;
        sesion.FechaExpiracion = DateTime.UtcNow.AddDays(30);
        sesion.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        await _context.SaveChangesAsync();

        return Result(new
        {
            Token = tokenData.Token,
            RefreshToken = newRefreshToken,
            Permisos = permisosDict
        }, "Token renovado exitosamente.");
    }

    [Authorize]
    [HttpPost("logout")]
    [EndpointSummary("Cerrar sesión actual")]
    public async Task<IActionResult> Logout()
    {
        var jti = _currentUser.Jti; // Uso directo del servicio

        if (string.IsNullOrEmpty(jti)) return Error("Sesión no identificada.");

        await _cacheService.SetAsync($"revoked_{jti}", true, TimeSpan.FromHours(1));

        var sesion = await _context.UsuarioSesiones.FirstOrDefaultAsync(s => s.Jti == jti);
        if (sesion != null)
        {
            _context.UsuarioSesiones.Remove(sesion);
            await _context.SaveChangesAsync();
        }

        return Result(true, "Has cerrado sesión correctamente.");
    }

    #region CONFIRMACIÓN DE CORREO


    [HttpPost("enviar-codigo-confirmacion")]
    [EndpointSummary("Genera y envía un código de 6 dígitos al correo del usuario")]
    public async Task<IActionResult> EnviarCodigoConfirmacion([FromBody] EnviarCodigoRequest request)
    {
        var user = await _context.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (user == null)
            return Error("Usuario no encontrado.");

        if (user.CorreoConfirmado)
            return Error("Este correo ya se encuentra confirmado.");

        // 1. Generamos el código y su fecha de expiración (15 minutos)
        var codigo = new Random().Next(100000, 999999).ToString();
        var expiracion = DateTime.UtcNow.AddMinutes(15).ToString("O"); // Formato ISO 8601 exacto

        // 2. Lo guardamos en BD empaquetado (Ej: "123456|2026-03-23T15:30:00.0000000Z")
        user.TokenConfirmacionCorreo = $"{codigo}|{expiracion}";
        await _context.SaveChangesAsync();

        // 3. USAMOS NUESTRO NUEVO TEMPLATE (Al template solo le mandamos el código limpio)
        string html = EmailTemplates.ConfirmacionCorreo(user.Nombre, codigo);

        // 4. Enviamos el correo
        bool enviado = await _emailService.EnviarCorreoAsync(user.Correo, "Código de validación de cuenta", html);

        if (!enviado)
            return Error("Ocurrió un error al intentar enviar el correo. Por favor, intenta de nuevo más tarde.");

        return Result(true, "Código enviado exitosamente al correo proporcionado.");
    }

    [HttpPost("verificar-correo")]
    [EndpointSummary("Valida el código y marca el correo como confirmado")]
    public async Task<IActionResult> VerificarCorreo([FromBody] VerificarCodigoRequest request)
    {
        // También usamos IgnoreQueryFilters aquí para evitar que Entity Framework oculte al usuario
        var user = await _context.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (user == null)
            return Error("Usuario no encontrado.");

        if (user.CorreoConfirmado)
            return Error("Este correo ya se encuentra confirmado.");

        // Validamos que tenga un token guardado
        if (string.IsNullOrEmpty(user.TokenConfirmacionCorreo))
            return Error("No tienes ningún código de confirmación pendiente.");

        // Desempaquetamos el valor de la BD (Ej: "123456|2026-03-23T15:30:00.0000000Z")
        var partesToken = user.TokenConfirmacionCorreo.Split('|');

        // Validamos que el formato sea el esperado (2 partes) y que el código numérico coincida
        if (partesToken.Length != 2 || partesToken[0] != request.Codigo)
            return Error("El código ingresado es incorrecto.");

        // Validamos la fecha de expiración
        if (DateTime.TryParse(partesToken[1], out var fechaExpiracion))
        {
            // EL FIX: Agregamos .ToUniversalTime() para comparar peras con peras (UTC vs UTC)
            if (DateTime.UtcNow > fechaExpiracion.ToUniversalTime())
            {
                // Si ya expiró, lo borramos de la BD por seguridad
                user.TokenConfirmacionCorreo = null;
                await _context.SaveChangesAsync();

                return Error("El código ha expirado. Por favor, solicita uno nuevo.");
            }
        }
        else
        {
            return Error("El código almacenado tiene un formato de fecha inválido.");
        }

        // Si llega aquí: El código es correcto y NO ha expirado
        user.CorreoConfirmado = true;
        user.TokenConfirmacionCorreo = null; // Limpiamos para que no se pueda reusar

        await _context.SaveChangesAsync();

        return Result(true, "Tu correo ha sido confirmado exitosamente.");
    }
    #endregion

    #endregion

    #region SEGURIDAD 2FA (TOTP)

    [Authorize]
    [HttpPost("setup-2fa")]
    [EndpointSummary("Genera las credenciales para configurar Google Authenticator/Authy")]
    public async Task<IActionResult> Setup2FA()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Error("Usuario no identificado.");

        var user = await _context.Usuarios.FindAsync(int.Parse(userId));
        if (user == null) return Error("Usuario no encontrado.");

        if (user.DosPasosHabilitado)
            return Error("La autenticación de dos pasos ya se encuentra habilitada en esta cuenta.");

        // 1. Generamos la llave usando nuestro nuevo servicio de Infraestructura
        var secretKey = _twoFactorService.GenerateSecretKey();

        // 2. Guardamos la llave, pero AÚN NO habilitamos el 2FA (hasta que compruebe que funciona)
        user.DosPasosSecretKey = secretKey;
        await _context.SaveChangesAsync();

        // 3. Generamos la URI para el Deep Link / Código QR
        // El Issuer es el nombre que saldrá en la app de Authenticator (ej. SPII_Empadronamiento)
        var qrUri = _twoFactorService.GenerateQrCodeUri(user.Correo, secretKey, "Spii.mx");

        // 4. Retornamos usando tu método Result<>
        return Result(new
        {
            LlaveManual = secretKey,
            EnlaceAuthenticator = qrUri
        }, "Credenciales 2FA generadas. Por favor, confírmalas ingresando el código generado por tu aplicación.");
    }

    [Authorize]
    [HttpPost("confirmar-setup-2fa")]
    [EndpointSummary("Valida el primer código de la app para habilitar el 2FA definitivamente")]
    public async Task<IActionResult> ConfirmarSetup2FA([FromBody] Confirmar2FARequest request)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId)) return Error("Usuario no identificado.");

        var user = await _context.Usuarios.FindAsync(int.Parse(userId));
        if (user == null) return Error("Usuario no encontrado.");

        if (user.DosPasosHabilitado)
            return Error("La autenticación de dos pasos ya se encuentra habilitada.");

        if (string.IsNullOrEmpty(user.DosPasosSecretKey))
            return Error("No has iniciado la configuración del 2FA. Por favor, solicita las credenciales primero.");

        // Validamos el código usando nuestro servicio
        bool esValido = _twoFactorService.ValidateCode(user.DosPasosSecretKey, request.Codigo);

        if (!esValido)
            return Error("El código ingresado es incorrecto o ha expirado. Intenta nuevamente.");

        // ¡Si pasa la validación, blindamos la cuenta!
        user.DosPasosHabilitado = true;
        await _context.SaveChangesAsync();

        return Result(true, "¡Excelente! La autenticación de dos pasos ha sido habilitada correctamente.");
    }

    #endregion

    #region SEGURIDAD DE CUENTA

    [Authorize]
    [HttpPut("update-password")]
    [EndpointSummary("Cambiar contraseña")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userId = _currentUser.UserId; // Uso directo del servicio
        if (string.IsNullOrEmpty(userId)) return Error("Usuario no identificado.");

        var user = await _context.Usuarios.FindAsync(int.Parse(userId));
        if (user == null) return Error("Usuario no encontrado.");

        user.PasswordHash = _passwordService.HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();

        return Result(true, "Tu contraseña ha sido actualizada con éxito.");
    }

    #endregion
}