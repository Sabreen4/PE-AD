namespace NotificationService.Application.Interfaces;

/// <summary>
/// Defines a service for broadcasting real-time notifications to connected clients.
/// </summary>
public interface INotificationBroadcaster
{
    /// <summary>
    /// Broadcasts a notification to a specific user in real time.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message body.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    Task BroadcastNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
}
