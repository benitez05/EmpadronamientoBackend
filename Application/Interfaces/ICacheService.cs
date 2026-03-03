namespace EmpadronamientoBackend.Application.Interfaces;

public interface ICacheService
{
    // Métodos estándar para manejar el caché
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}