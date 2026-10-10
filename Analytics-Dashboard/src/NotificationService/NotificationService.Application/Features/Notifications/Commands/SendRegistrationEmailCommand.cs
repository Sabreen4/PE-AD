using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Features.Notifications.Commands;

/// <summary>
/// Command to send a registration email and persist the associated notification record.
/// </summary>
/// <param name="Dto">The registration email request data.</param>
public sealed record SendRegistrationEmailCommand(SendRegistrationEmailRequestDto Dto) : IRequest<SendEmailResponseDto>;

/// <summary>
/// Handles the <see cref="SendRegistrationEmailCommand"/> by sending the email, persisting the notification, and broadcasting it.
/// </summary>
public sealed class SendRegistrationEmailCommandHandler : IRequestHandler<SendRegistrationEmailCommand, SendEmailResponseDto>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly INotificationTypeResolver _typeResolver;
    private readonly ILogger<SendRegistrationEmailCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendRegistrationEmailCommandHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="broadcaster">Service for broadcasting real-time notifications.</param>
    /// <param name="typeResolver">Service for resolving notification type identifiers.</param>
    /// <param name="logger">Logger instance.</param>
    public SendRegistrationEmailCommandHandler(
        INotificationService notificationService,
        IEmailService emailService,
        INotificationBroadcaster broadcaster,
        INotificationTypeResolver typeResolver,
        ILogger<SendRegistrationEmailCommandHandler> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _broadcaster = broadcaster;
        _typeResolver = typeResolver;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by sending a registration email, creating a notification record, and broadcasting the event.
    /// </summary>
    /// <param name="request">The send-registration-email command.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A response indicating success or failure.</returns>
    public async Task<SendEmailResponseDto> Handle(SendRegistrationEmailCommand request, CancellationToken cancellationToken)
    {
        SendRegistrationEmailRequestDto dto = request.Dto;
        _logger.LogInformation("Sending registration email to: {Email}", dto.Email);
        string subject = "Welcome to the Platform";
        string body = $"Hello {dto.FirstName},<br/><br/>"
            + $"Your account has been created. Please use the link below to log in:<br/><br/>"
            + $"<a href='{dto.LoginLink}'>Log in to your account</a><br/><br/>"
            + $"Thank you,<br/>The Team";

        await _emailService.SendEmailAsync(dto.Email, subject, body);

        Guid registrationTypeId = await _typeResolver.ResolveAsync("REGISTRATION_EMAIL", cancellationToken);

        Notification notification = new Notification
        {
            NotificationTypeId = registrationTypeId,
            Title = subject,
            Message = $"Registration email sent to {dto.Email}",
        };

        await _notificationService.CreateNotificationAsync(notification);
        await _notificationService.SaveAsync();

        await _broadcaster.BroadcastNotificationAsync("", subject, notification.Message, cancellationToken);

        return new SendEmailResponseDto
        {
            Success = true,
            Message = "Registration email sent successfully.",
            NotificationId = notification.Id,
        };
    }
}