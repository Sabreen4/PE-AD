using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Redis.Interfaces;
using StackExchange.Redis;

namespace Shared.Redis.Services;

/// <summary>
/// Redis-backed implementation of <see cref="ICacheService"/>.
/// This is the only class allowed to reference IConnectionMultiplexer/IDatabase
/// directly; every other consumer depends on <see cref="ICacheService"/>.
/// Uses IConnectionMultiplexer (not IDistributedCache) because prefix-based
/// removal requires server-side key scanning, which IDistributedCache doesn't expose.
/// </summary>
public sealed class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly Shared.Redis.Options.RedisCacheOptions options;
        private readonly ILogger<RedisCacheService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
        /// </summary>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        /// <param name="options">The Redis cache configuration options.</param>
        /// <param name="logger">The logger instance.</param>
        public RedisCacheService(
            IConnectionMultiplexer connectionMultiplexer,
            IOptions<Shared.Redis.Options.RedisCacheOptions> options,
            ILogger<RedisCacheService> logger)
        {
            this.connectionMultiplexer = connectionMultiplexer;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                IDatabase database = this.connectionMultiplexer.GetDatabase();
                RedisValue value = await database.StringGetAsync(this.BuildKey(key));

                if (value.IsNullOrEmpty)
                {
                    this.logger.LogInformation("Cache miss for key {Key}", key);
                    return null;
                }

                this.logger.LogInformation("Cache hit for key {Key}", key);
                return JsonSerializer.Deserialize<T>((string)value!);
            }
            catch (RedisConnectionException redisConnectionException)
            {
                this.logger.LogWarning(redisConnectionException, "Redis unavailable while getting key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            string serializedValue = JsonSerializer.Serialize(value);
            TimeSpan effectiveExpiration = expiration ?? TimeSpan.FromMinutes(this.options.DefaultExpirationMinutes);

            await database.StringSetAsync(this.BuildKey(key), serializedValue, effectiveExpiration);
        }
        catch (RedisConnectionException redisConnectionException)
        {
            // Caching is best-effort: a Redis outage must never break the caller.
            this.logger.LogWarning(redisConnectionException, "Redis unavailable while setting key {Key}", key);
        }
    }

        /// <summary>
        /// Removes a single cache entry by its key.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <param name="cancellationToken">Token to cancel the async operation.</param>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                IDatabase database = this.connectionMultiplexer.GetDatabase();
                await database.KeyDeleteAsync(this.BuildKey(key));
                this.logger.LogInformation("Removed cache key {Key}", key);
            }
            catch (RedisConnectionException redisConnectionException)
            {
                this.logger.LogWarning(redisConnectionException, "Redis unavailable while removing key {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            string fullPrefix = this.BuildKey(prefix);

            foreach (EndPoint endpoint in this.connectionMultiplexer.GetEndPoints())
            {
                IServer server = this.connectionMultiplexer.GetServer(endpoint);
                IDatabase database = this.connectionMultiplexer.GetDatabase();

                await foreach (RedisKey key in server.KeysAsync(pattern: fullPrefix + "*"))
                {
                    await database.KeyDeleteAsync(key);
                }
            }
        }
        catch (RedisConnectionException redisConnectionException)
        {
            this.logger.LogWarning(redisConnectionException, "Redis unavailable while removing keys with prefix {Prefix}", prefix);
        }
    }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default) where T : class
        {
            T? cachedValue = await this.GetAsync<T>(key, cancellationToken);
            if (cachedValue is not null)
            {
                this.logger.LogInformation("Returning cached value for key {Key}", key);
                return cachedValue;
            }

            this.logger.LogInformation("Cache miss for key {Key}, invoking factory", key);
            T freshValue = await factory(cancellationToken);
            await this.SetAsync(key, freshValue, expiration, cancellationToken);

            return freshValue;
        }

    private string BuildKey(string key)
    {
        return string.IsNullOrEmpty(this.options.InstanceName) ? key : $"{this.options.InstanceName}{key}";
    }
}