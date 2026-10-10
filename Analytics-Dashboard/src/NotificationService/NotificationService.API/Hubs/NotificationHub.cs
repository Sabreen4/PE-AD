using Microsoft.AspNetCore.SignalR;

namespace NotificationService.API.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery to connected clients.
/// </summary>
public sealed class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHub"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds the calling connection to a named group for targeted notifications.
    /// </summary>
    /// <param name="groupName">The name of the group to join.</param>
    public async Task JoinGroup(string groupName)
    {
        _logger.LogInformation("Connection {ConnectionId} joining group {GroupName}", Context.ConnectionId, groupName);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Removes the calling connection from a named group.
    /// </summary>
    /// <param name="groupName">The name of the group to leave.</param>
    public async Task LeaveGroup(string groupName)
    {
        _logger.LogInformation("Connection {ConnectionId} leaving group {GroupName}", Context.ConnectionId, groupName);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
