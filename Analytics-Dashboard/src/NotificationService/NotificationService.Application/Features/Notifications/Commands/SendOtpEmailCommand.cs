using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Features.Notifications.Commands;

/// <summary>
/// Command to send an OTP email and persist the associated notification record.
/// </summary>
/// <param name="Dto">The OTP email request data.</param>
public sealed record SendOtpEmailCommand(SendOtpEmailRequestDto Dto) : IRequest<SendEmailResponseDto>;

/// <summary>
/// Handles the <see cref="SendOtpEmailCommand"/> by sending the OTP email, persisting the notification, and broadcasting it.
/// </summary>
public sealed class SendOtpEmailCommandHandler : IRequestHandler<SendOtpEmailCommand, SendEmailResponseDto>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly INotificationTypeResolver _typeResolver;
    private readonly ILogger<SendOtpEmailCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendOtpEmailCommandHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="broadcaster">Service for broadcasting real-time notifications.</param>
    /// <param name="typeResolver">Service for resolving notification type identifiers.</param>
    /// <param name="logger">Logger instance.</param>
    public SendOtpEmailCommandHandler(
        INotificationService notificationService,
        IEmailService emailService,
        INotificationBroadcaster broadcaster,
        INotificationTypeResolver typeResolver,
        ILogger<SendOtpEmailCommandHandler> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _broadcaster = broadcaster;
        _typeResolver = typeResolver;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by sending an OTP email, creating a notification record, and broadcasting the event.
    /// </summary>
    /// <param name="request">The send-OTP-email command.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A response indicating success or failure.</returns>
    public async Task<SendEmailResponseDto> Handle(SendOtpEmailCommand request, CancellationToken cancellationToken)
    {
        SendOtpEmailRequestDto dto = request.Dto;
        _logger.LogInformation("Sending OTP email to: {Email}", dto.Email);
        string subject = "Your OTP Code";
        string body = $"Your OTP is: <strong>{dto.Otp}</strong><br/><br/>"
            + $"This code will expire in {dto.ExpiryMinutes} minutes.<br/><br/>"
            + $"If you did not request this, please ignore this email.<br/><br/>"
            + $"Thank you,<br/>The Team";

        Guid otpTypeId = await _typeResolver.ResolveAsync("OTP_EMAIL", cancellationToken);
        await _emailService.SendEmailAsync(dto.Email, subject, body);

        Notification notification = new Notification
        {
            NotificationTypeId = otpTypeId,
            Title = subject,
            Message = $"OTP email sent to {dto.Email}",
        };

        await _notificationService.CreateNotificationAsync(notification);
        await _notificationService.SaveAsync();

        await _broadcaster.BroadcastNotificationAsync("", subject, notification.Message, cancellationToken);

        return new SendEmailResponseDto
        {
            Success = true,
            Message = "OTP email sent successfully.",
            NotificationId = notification.Id,
        };
    }
}