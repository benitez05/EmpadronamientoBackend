using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BenitezLabs.Domain.Entities;
using EmpadronamientoBackend.Application.Interfaces;

namespace EmpadronamientoBackend.Infrastructure.Identity;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IConfiguration _configuration;

    public PasswordService(IConfiguration configuration)
    {
        _passwordHasher = new PasswordHasher<Usuario>();
        _configuration = configuration;
    }

    public string HashPassword(Usuario user, string password) =>
        _passwordHasher.HashPassword(user, password);

    public bool IsValidPassword(Usuario user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public TokenResponse GenerateJwtToken(Usuario user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = Guid.NewGuid().ToString();

        // Mapeo de permisos para el diccionario
        var permisosDict = user.Role?.Permisos?
            .Where(p => p.Modulo != null)
            .ToDictionary(p => p.Modulo.K, p => p.Lvl)
            ?? new Dictionary<string, int>();

        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Sub, user.Id.ToString() },
            { JwtRegisteredClaimNames.Jti, jti },
            { JwtRegisteredClaimNames.Email, user.Correo },
            { "role", user.Role?.Nombre ?? "Usuario" },
            
            // 1. EL BYPASS: Nivel de seguridad (1, 2 o 3)
            // Esto es lo que el atributo [AuthLvl] leerá para el Modo Dios
            { "tipo", user.Tipo.ToString() }, 

            // 2. MULTI-TENANCY: ID de organización
            { "OrganizacionId", user.OrganizacionId.ToString() }, 

            // 3. PERMISOS: Serializados para que el atributo los pueda leer
            { "permisos", JsonSerializer.Serialize(permisosDict) }, 

            { "iss", _configuration["Jwt:Issuer"] },
            { "aud", _configuration["Jwt:Audience"] },
            { "exp", (int)DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() }
        };

        var header = new JwtHeader(creds);
        var token = new JwtSecurityToken(header, payload);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse(tokenString, jti);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}