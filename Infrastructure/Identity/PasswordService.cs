using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BenitezLabs.Domain.Entities; // Tu modelo Usuario

namespace EmpadronamientoBackend.Infrastructure.Identity;

public class PasswordService
{
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly double _jwtExpirationInMinutes = 60;

    public PasswordService(IConfiguration configuration)
    {
        _passwordHasher = new PasswordHasher<Usuario>();
        _configuration = configuration;
    }

    public string HashPassword(Usuario user, string password) =>
        _passwordHasher.HashPassword(user, password);

    public PasswordVerificationResult VerifyPassword(Usuario user, string password) =>
        _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

    public string GenerateJwtToken(Usuario user)
    {
        // 1. Claims básicos de identidad
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Correo),
        
        // El Rol es fundamental para el atributo [HasRole]
        new Claim(ClaimTypes.Role, user.Role.Nombre)
    };

        // 2. Inyección de Permisos Modulares
        // Recorremos los permisos del Rol que tiene asignado el usuario
        if (user.Role?.Permisos != null)
        {
            foreach (var permiso in user.Role.Permisos)
            {
                // Metemos la Key del módulo (ej: "u") y su nivel (ej: "3")
                // Esto es lo que lee tu atributo [AuthLvl(k, l)]
                claims.Add(new Claim(permiso.Modulo.K, permiso.Lvl.ToString()));
            }
        }

        // 3. Configuración de la llave y firma
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Creación del objeto Token
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}