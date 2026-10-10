using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Redis.Interfaces;
using Shared.Redis.Options;
using Shared.Redis.Services;
using StackExchange.Redis;

namespace Shared.Redis.Extensions;

/// <summary>
/// Provides extension methods for registering shared Redis caching services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared Redis <see cref="IConnectionMultiplexer"/> and
    /// <see cref="ICacheService"/>, the wrapper other services should inject
    /// to read/write cached values.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application configuration containing the Redis section.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Redis connection string is not configured.
    /// </exception>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisCacheOptions>(configuration.GetSection("Redis"));

        RedisCacheOptions redisCacheOptions = new RedisCacheOptions();
        configuration.GetSection("Redis").Bind(redisCacheOptions);

        if (string.IsNullOrWhiteSpace(redisCacheOptions.ConnectionString))
        {
            throw new InvalidOperationException("Missing Redis:ConnectionString in configuration.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisCacheOptions.ConnectionString));

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
