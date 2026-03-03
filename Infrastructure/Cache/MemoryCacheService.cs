// Proyecto: Infrastructure/Cache/MemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using EmpadronamientoBackend.Application.Interfaces;

namespace EmpadronamientoBackend.Infrastructure.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // Si no mandas expiración, le ponemos 1 hora por defecto
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
        };

        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}