using BenitezLabs.Domain.Entities;

namespace EmpadronamientoBackend.Application.Interfaces;

public record TokenResponse(string Token, string Jti);

public interface IPasswordService
{
    string HashPassword(Usuario user, string password);
    
    // Cambiamos el enum de Identity por un simple bool o un enum propio
    bool IsValidPassword(Usuario user, string password);
    
    TokenResponse GenerateJwtToken(Usuario user);
    string GenerateRefreshToken();
}