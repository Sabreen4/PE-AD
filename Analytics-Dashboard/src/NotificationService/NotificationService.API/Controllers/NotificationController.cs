using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Application.Features.Notifications.Commands;
using NotificationService.Application.Features.Notifications.Queries;
using Shared.Authorization.Interfaces;

namespace NotificationService.API.Controllers;

/// <summary>
/// API controller for managing notifications, including retrieval, read state management, deletion, and email sending.
/// </summary>
[ApiController]
[Authorize]
[Route("notifications")]
public sealed class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContext;
    private readonly ILogger<NotificationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR sender for dispatching commands and queries.</param>
    /// <param name="userContext">Service to retrieve the current user context.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationController(IMediator mediator, IUserContextService userContext, ILogger<NotificationController> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of notifications for the current user, optionally filtered by read state.
    /// </summary>
    /// <param name="isRead">Optional filter for read/unread notifications.</param>
    /// <param name="pageNumber">The page number to retrieve (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 10).</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A paginated response containing notification items.</returns>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        Guid userId = _userContext.UserId ?? Guid.Empty;
        _logger.LogInformation("Getting notifications for user {UserId} with isRead {IsRead}, page {PageNumber} size {PageSize} search '{Search}'", userId, isRead, pageNumber, pageSize, search);
        PagedNotificationResponseDto result = await _mediator.Send(
            new GetNotificationsQuery(userId, isRead, pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single notification by its ID for the current user.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The notification details.</returns>
    [HttpGet("{notificationId:long}")]
    public async Task<IActionResult> GetNotificationById(
        long notificationId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? Guid.Empty;
        _logger.LogInformation("Getting notification {NotificationId} for user {UserId}", notificationId, userId);
        NotificationResponseDto result = await _mediator.Send(
            new GetNotificationByIdQuery(notificationId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Marks a specific notification as read for the current user.
    /// </summary>
    /// <param name="notificationId">The notification ID to mark as read.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    [HttpPut("{notificationId:long}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(
        long notificationId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? Guid.Empty;
        _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
        await _mediator.Send(new MarkAsReadCommand(notificationId, userId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks all notifications as read for the current user.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    [HttpPut("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? Guid.Empty;
        _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);
        await _mediator.Send(new MarkAllAsReadCommand(userId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a specific notification for the current user.
    /// </summary>
    /// <param name="notificationId">The notification ID to delete.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    [HttpDelete("{notificationId:long}")]
    public async Task<IActionResult> DeleteNotification(
        long notificationId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? Guid.Empty;
        _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", notificationId, userId);
        await _mediator.Send(new DeleteNotificationCommand(notificationId, userId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sends a registration email to the specified address.
    /// </summary>
    /// <param name="dto">The registration email request data.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The result of the email send operation.</returns>
    [HttpPost("send-registration-email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendRegistrationEmail([FromBody] SendRegistrationEmailRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending registration email to {Email}", dto.Email);
        SendEmailResponseDto result = await _mediator.Send(new SendRegistrationEmailCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sends an OTP email to the specified address.
    /// </summary>
    /// <param name="dto">The OTP email request data.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The result of the email send operation.</returns>
    [HttpPost("send-otp-email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtpEmail([FromBody] SendOtpEmailRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending OTP email to {Email}", dto.Email);
        SendEmailResponseDto result = await _mediator.Send(new SendOtpEmailCommand(dto), cancellationToken);
        return Ok(result);
    }
}