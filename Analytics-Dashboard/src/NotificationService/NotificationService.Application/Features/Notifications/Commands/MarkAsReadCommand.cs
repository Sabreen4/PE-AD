using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Entities;
using Shared.Exceptions;

namespace NotificationService.Application.Features.Notifications.Commands;

/// <summary>
/// Command to mark a specific notification as read for the current user.
/// </summary>
/// <param name="UserNotificationId">The user notification ID to mark as read.</param>
/// <param name="UserId">The user's ID for ownership validation.</param>
public sealed record MarkAsReadCommand(long UserNotificationId, Guid UserId) : IRequest;

/// <summary>
/// Handles the <see cref="MarkAsReadCommand"/> by updating the read state of the user notification.
/// </summary>
public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<MarkAsReadCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkAsReadCommandHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">Logger instance.</param>
    public MarkAsReadCommandHandler(INotificationService notificationService, ILogger<MarkAsReadCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by finding the user notification, marking it as read, and persisting the change.
    /// </summary>
    /// <param name="request">The mark-as-read command.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    public async Task Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking notification as read: {UserNotificationId} for UserId: {UserId}", request.UserNotificationId, request.UserId);

        UserNotification? userNotification = await _notificationService.GetUserNotificationByIdAsync(
            request.UserNotificationId, request.UserId);

        if (userNotification is null)
        {
            _logger.LogWarning("Notification not found: {UserNotificationId}", request.UserNotificationId);
            throw new NotFoundException("Notification not found.");
        }

        userNotification.IsRead = true;
        userNotification.ReadAt = DateTime.UtcNow;

        _notificationService.UpdateUserNotification(userNotification);
        await _notificationService.SaveAsync();
    }
}