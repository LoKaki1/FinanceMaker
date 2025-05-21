using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FinanceMaker.Broker.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache m_Cache;
        private readonly ILogger<RedisCacheService> m_Logger;
        private readonly JsonSerializerOptions m_JsonOptions;

        public RedisCacheService(
            IDistributedCache cache,
            ILogger<RedisCacheService> logger)
        {
            m_Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await m_Cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(cachedValue, m_JsonOptions);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error retrieving value from cache for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, m_JsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }

                await m_Cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error setting value in cache for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await m_Cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error removing value from cache for key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await m_Cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error checking existence in cache for key {Key}", key);
                return false;
            }
        }
    }
}
