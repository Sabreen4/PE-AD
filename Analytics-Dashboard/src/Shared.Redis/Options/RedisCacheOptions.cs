namespace Shared.Redis.Options;

/// <summary>
/// Bind this to a "Redis" section in appsettings.json, e.g.:
/// "Redis": {
///   "ConnectionString": "localhost:6379",
///   "InstanceName": "InterviewService:",
///   "DefaultExpirationMinutes": 10
/// }
/// </summary>
public sealed class RedisCacheOptions
{
    /// <summary>Gets or sets the Redis connection string (e.g. "localhost:6379").</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Prefixed onto every key so services sharing one Redis instance
    /// don't collide (e.g. "InterviewService:", "MetaDataService:").
    /// </summary>
    public string InstanceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the default cache expiration time in minutes.</summary>
    public int DefaultExpirationMinutes { get; set; } = 10;
}
