namespace Shared.Redis.Interfaces;

/// <summary>
/// Abstraction over the Redis-backed cache. Callers depend on this, never
/// on StackExchange.Redis or IDistributedCache directly.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or <see langword="null"/>
    /// if it doesn't exist or the cache is unavailable.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Stores <paramref name="value"/> under <paramref name="key"/>. Uses the
    /// configured default expiration if <paramref name="expiration"/> is not supplied.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>Removes a single cache entry by its key.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes every key starting with <paramref name="prefix"/>. Use for
    /// invalidating a whole group at once, e.g. all RefTerms for one RefSet.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache-aside helper: returns the cached value if present; otherwise
    /// calls <paramref name="factory"/>, caches the result, and returns it.
    /// This is the method most call sites should use.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;
}
