using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Domain.Entities;
using Shared.Exceptions;

namespace NotificationService.Application.Features.Notifications.Queries;

/// <summary>
/// Query to retrieve a single notification by its ID for a specific user.
/// </summary>
/// <param name="UserNotificationId">The user notification ID.</param>
/// <param name="UserId">The user's ID for ownership validation.</param>
public sealed record GetNotificationByIdQuery(long UserNotificationId, Guid UserId) : IRequest<NotificationResponseDto>;

/// <summary>
/// Handles the <see cref="GetNotificationByIdQuery"/> by validating ownership and returning the notification details.
/// </summary>
public sealed class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, NotificationResponseDto>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<GetNotificationByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNotificationByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">Logger instance.</param>
    public GetNotificationByIdQueryHandler(INotificationService notificationService, ILogger<GetNotificationByIdQueryHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query and returns the notification details, or throws a not-found exception.
    /// </summary>
    /// <param name="request">The get-notification-by-id query.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The notification response DTO.</returns>
    public async Task<NotificationResponseDto> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification by id: {UserNotificationId} for UserId: {UserId}", request.UserNotificationId, request.UserId);

        UserNotification? userNotification = await _notificationService.GetUserNotificationByIdAsync(
            request.UserNotificationId, request.UserId);

        if (userNotification is null)
        {
            _logger.LogWarning("Notification not found: {UserNotificationId}", request.UserNotificationId);
            throw new NotFoundException("Notification not found.");
        }

        return new NotificationResponseDto
        {
            Id = userNotification.Id,
            Title = userNotification.Notification.Title,
            Message = userNotification.Notification.Message,
            Channel = "IN_APP",
            IsRead = userNotification.IsRead,
            CreatedAt = userNotification.CreatedAt,
        };
    }
}