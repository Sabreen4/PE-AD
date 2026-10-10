using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Features.Notifications.Commands;

/// <summary>
/// Command to mark all unread notifications as read for the current user.
/// </summary>
/// <param name="UserId">The user's ID.</param>
public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest;

/// <summary>
/// Handles the <see cref="MarkAllAsReadCommand"/> by updating all unread user notifications to read.
/// </summary>
public sealed class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<MarkAllAsReadCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkAllAsReadCommandHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">Logger instance.</param>
    public MarkAllAsReadCommandHandler(INotificationService notificationService, ILogger<MarkAllAsReadCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by fetching all unread notifications for the user and marking each as read.
    /// </summary>
    /// <param name="request">The mark-all-as-read command.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    public async Task Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking all notifications as read for UserId: {UserId}", request.UserId);
        List<UserNotification> unread = await _notificationService.GetUnreadNotificationsAsync(request.UserId);

        DateTime utcNow = DateTime.UtcNow;
        foreach (UserNotification item in unread)
        {
            item.IsRead = true;
            item.ReadAt = utcNow;
            _notificationService.UpdateUserNotification(item);
        }

        await _notificationService.SaveAsync();
    }
}