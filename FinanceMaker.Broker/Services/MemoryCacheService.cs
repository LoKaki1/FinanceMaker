using System.Text.Json;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FinanceMaker.Broker.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache m_Cache;
    private readonly MemoryCacheEntryOptions m_DefaultOptions;

    public MemoryCacheService(IMemoryCache cache)
    {
        m_Cache = cache;
        m_DefaultOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        if (m_Cache.TryGetValue(key, out T? value))
        {
            return Task.FromResult(value);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration)
            .SetAbsoluteExpiration(expiration.Add(TimeSpan.FromMinutes(5)));

        m_Cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        m_Cache.Remove(key);
        return Task.CompletedTask;
    }
}
