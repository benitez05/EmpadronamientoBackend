using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
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

    // Cambiamos el nombre y el tipo de retorno para cumplir con la interfaz
    public bool IsValidPassword(Usuario user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        // Retornamos true si la contraseña es correcta o si es correcta pero necesita re-hashearse
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public TokenResponse GenerateJwtToken(Usuario user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = Guid.NewGuid().ToString();

        // 🚀 MEJORA: Generamos un diccionario plano para que sea fácil de leer en el Atributo
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
        { "permisos", permisosDict }, // Se serializa como: {"u":1, "r":1}
        { "iss", _configuration["Jwt:Issuer"] },
        { "aud", _configuration["Jwt:Audience"] },
        { "exp", (int)DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds() }
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