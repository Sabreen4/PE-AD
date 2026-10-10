using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Entities;
using Shared.Exceptions;

namespace NotificationService.Application.Features.Notifications.Commands;

/// <summary>
/// Command to delete a specific notification for the current user.
/// </summary>
/// <param name="UserNotificationId">The user notification ID to delete.</param>
/// <param name="UserId">The user's ID for ownership validation.</param>
public sealed record DeleteNotificationCommand(long UserNotificationId, Guid UserId) : IRequest;

/// <summary>
/// Handles the <see cref="DeleteNotificationCommand"/> by finding and removing the user notification.
/// </summary>
public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNotificationCommandHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">Logger instance.</param>
    public DeleteNotificationCommandHandler(INotificationService notificationService, ILogger<DeleteNotificationCommandHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by finding the user notification, deleting it, and persisting the change.
    /// </summary>
    /// <param name="request">The delete-notification command.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting notification: {UserNotificationId} for UserId: {UserId}", request.UserNotificationId, request.UserId);

        UserNotification? userNotification = await _notificationService.GetUserNotificationByIdAsync(
            request.UserNotificationId, request.UserId);

        if (userNotification is null)
        {
            _logger.LogWarning("Notification not found: {UserNotificationId}", request.UserNotificationId);
            throw new NotFoundException("Notification not found.");
        }

        _notificationService.DeleteUserNotification(userNotification);
        await _notificationService.SaveAsync();
    }
}