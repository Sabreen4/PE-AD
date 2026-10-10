namespace NotificationService.Application.Interfaces;

/// <summary>
/// Resolves a notification type string key to its corresponding GUID identifier.
/// </summary>
public interface INotificationTypeResolver
{
    /// <summary>
    /// Resolves the given key to a GUID representing the notification type.
    /// </summary>
    /// <param name="key">The notification type string key.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The GUID identifier for the notification type.</returns>
    Task<Guid> ResolveAsync(String key, CancellationToken cancellationToken = default);
}
