using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Features.Notifications.Queries;

/// <summary>
/// Query to retrieve a paginated list of notifications for a user, optionally filtered by read state.
/// </summary>
/// <param name="UserId">The user's ID.</param>
/// <param name="IsRead">Optional filter for read/unread state.</param>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
public sealed record GetNotificationsQuery(Guid UserId, bool? IsRead, int PageNumber, int PageSize, string? Search = null) : IRequest<PagedNotificationResponseDto>;

/// <summary>
/// Handles the <see cref="GetNotificationsQuery"/> by fetching, filtering, and paginating user notifications.
/// </summary>
public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedNotificationResponseDto>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<GetNotificationsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNotificationsQueryHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">Logger instance.</param>
    public GetNotificationsQueryHandler(INotificationService notificationService, ILogger<GetNotificationsQueryHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query and returns a paginated response of notifications.
    /// </summary>
    /// <param name="request">The get-notifications query.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A paginated notification response DTO.</returns>
    public async Task<PagedNotificationResponseDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notifications for UserId: {UserId}, IsRead: {IsRead}, Search: {Search}", request.UserId, request.IsRead, request.Search);
        List<UserNotification> items = await _notificationService.GetUserNotificationsAsync(request.UserId, request.IsRead);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            items = items
                .Where(un => un.Notification.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || un.Notification.Message.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        int totalRecords = items.Count;
        int totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        List<UserNotification> paged = items
            .OrderByDescending(un => un.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<NotificationResponseDto> mapped = paged
            .Select(un => new NotificationResponseDto
            {
                Id = un.Id,
                Title = un.Notification.Title,
                Message = un.Notification.Message,
                Channel = "IN_APP",
                IsRead = un.IsRead,
                CreatedAt = un.CreatedAt,
            })
            .ToList();

        return new PagedNotificationResponseDto
        {
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages,
        };
    }
}