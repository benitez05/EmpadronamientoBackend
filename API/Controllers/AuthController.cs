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

namespace EmpadronamientoBackend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        ApplicationDbContext context,
        IPasswordService passwordService,
        ICacheService cacheService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _passwordService = passwordService;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    #region REGISTRO Y LOGIN

    [AuthLvl("u", 2)]
    [HttpPost("registrar-usuario")]
    public async Task<IActionResult> RegisterStaff([FromBody] RegisterRequest request)
    {
        // 1. Verificación de duplicados
        if (await _context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Correo == request.Correo))
            return Error("Ya existe una cuenta asociada a este correo electrónico.");

        var orgId = _currentUser.OrganizacionId ?? 0;
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