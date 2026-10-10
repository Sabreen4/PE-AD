using Microsoft.AspNetCore.SignalR;
using NotificationService.Application.Interfaces;

namespace NotificationService.API.Hubs;

/// <summary>
/// Broadcasts real-time notifications to users via SignalR groups.
/// </summary>
public sealed class NotificationBroadcaster : INotificationBroadcaster
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationBroadcaster> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationBroadcaster"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for sending messages.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationBroadcaster(IHubContext<NotificationHub> hubContext, ILogger<NotificationBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Sends a real-time notification to a specific user's SignalR group.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message body.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    public async Task BroadcastNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Broadcasting notification to user {UserId} - Title: {Title}", userId, title);
        await _hubContext.Clients.Group(userId).SendAsync("Notification", new
        {
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        _logger.LogInformation("Notification broadcasted to user {UserId} successfully", userId);
    }
}
