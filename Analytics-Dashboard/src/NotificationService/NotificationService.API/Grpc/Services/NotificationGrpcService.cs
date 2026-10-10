using Grpc.Core;
using NotificationService.Application.Contracts;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using S_Notification = Shared.InterServiceHelper.Grpc.Notification;

namespace NotificationService.API.Grpc.Services;

/// <summary>
/// gRPC service for sending notifications (registration emails, OTP emails, and in-app notifications) from internal services.
/// </summary>
public sealed class NotificationGrpcService : S_Notification.NotificationService.NotificationServiceBase
{
    private readonly IRepoWrapper _repo;
    private readonly IEmailService _emailService;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly INotificationTypeResolver _typeResolver;
    private readonly ILogger<NotificationGrpcService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationGrpcService"/> class.
    /// </summary>
    /// <param name="repo">Unit-of-work wrapper for data access.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="broadcaster">Service for broadcasting real-time notifications.</param>
    /// <param name="typeResolver">Service for resolving notification type identifiers.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationGrpcService(
        IRepoWrapper repo,
        IEmailService emailService,
        INotificationBroadcaster broadcaster,
        INotificationTypeResolver typeResolver,
        ILogger<NotificationGrpcService> logger)
    {
        _repo = repo;
        _emailService = emailService;
        _broadcaster = broadcaster;
        _typeResolver = typeResolver;
        _logger = logger;
    }

    /// <summary>
    /// Handles a gRPC request to send a registration email and persists a corresponding notification record.
    /// </summary>
    /// <param name="request">The registration email request containing email, first name, and login link.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>A response indicating success or failure.</returns>
    public override async Task<S_Notification.SendEmailResponse> SendRegistrationEmail(
        S_Notification.SendRegistrationEmailRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("SendRegistrationEmail request received for {Email}", request.Email);

        try
        {
            String subject = "Welcome to the Platform";
            String body = $"Hello {request.FirstName},<br/><br/>" +
                          $"Your account has been created. Please use the link below to log in:<br/><br/>" +
                          $"<a href='{request.LoginLink}'>Log in to your account</a><br/><br/>" +
                          $"Thank you,<br/>The Team";

            await _emailService.SendEmailAsync(request.Email, subject, body);
            _logger.LogInformation("Registration email sent to {Email}", request.Email);

            Guid registrationTypeId = await _typeResolver.ResolveAsync("Email", context.CancellationToken);

            Notification notification = new Notification
            {
                NotificationTypeId = registrationTypeId,
                Title = subject,
                Message = $"Registration email sent to {request.Email}"
            };

            await _repo.Notification.CreateAsync(notification);

            _logger.LogInformation("Saving notification: TypeId={NotificationTypeId} Title={Title} Message={Message}",
                notification.NotificationTypeId, notification.Title, notification.Message);

            await _repo.SaveAsync();

            _logger.LogInformation("Notification saved successfully: Id={NotificationId}", notification.Id);

            await _broadcaster.BroadcastNotificationAsync("", subject, notification.Message, context.CancellationToken);

            return new S_Notification.SendEmailResponse
            {
                Success = true,
                Message = "Registration email sent successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration email to {Email}", request.Email);
            return new S_Notification.SendEmailResponse
            {
                Success = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
    }

    /// <summary>
    /// Handles a gRPC request to send an in-app notification to a specific user.
    /// </summary>
    /// <param name="request">The in-app notification request containing user ID, title, and message.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>A response indicating success or failure.</returns>
    public override async Task<S_Notification.SendInAppNotificationResponse> SendInAppNotification(
        S_Notification.SendInAppNotificationRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("SendInAppNotification request received for UserId={UserId}", request.UserId);

        try
        {
            Guid inAppTypeId = await _typeResolver.ResolveAsync("IN_APP", context.CancellationToken);

            Notification notification = new Notification
            {
                NotificationTypeId = inAppTypeId,
                Title = request.Title,
                Message = request.Message
            };

            await _repo.Notification.CreateAsync(notification);
            await _repo.SaveAsync();

            UserNotification userNotification = new UserNotification
            {
                UserId = Guid.Parse(request.UserId),
                NotificationId = notification.Id,
                IsRead = false
            };

            await _repo.UserNotification.CreateAsync(userNotification);
            await _repo.SaveAsync();

            await _broadcaster.BroadcastNotificationAsync(request.UserId, request.Title, request.Message, context.CancellationToken);

            return new S_Notification.SendInAppNotificationResponse
            {
                Success = true,
                Message = "In-app notification sent successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send in-app notification to {UserId}", request.UserId);
            return new S_Notification.SendInAppNotificationResponse
            {
                Success = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
    }

    /// <summary>
    /// Handles a gRPC request to send an OTP email and persists a corresponding notification record.
    /// </summary>
    /// <param name="request">The OTP email request containing email, OTP code, and expiry duration.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>A response indicating success or failure.</returns>
    public override async Task<S_Notification.SendEmailResponse> SendOtpEmail(
        S_Notification.SendOtpEmailRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("SendOtpEmail request received for {Email}", request.Email);

        try
        {
            String subject = "Your OTP Code";
            String body = $"Your OTP is: <strong>{request.Otp}</strong><br/><br/>" +
                          $"This code will expire in {request.ExpiryMinutes} minutes.<br/><br/>" +
                          $"If you did not request this, please ignore this email.<br/><br/>" +
                          $"Thank you,<br/>The Team";

            await _emailService.SendEmailAsync(request.Email, subject, body);
            _logger.LogInformation("OTP email sent to {Email}", request.Email);

            Guid otpTypeId = await _typeResolver.ResolveAsync("Email", context.CancellationToken);

            Notification notification = new Notification
            {
                NotificationTypeId = otpTypeId,
                Title = subject,
                Message = $"OTP email sent to {request.Email}"
            };

            await _repo.Notification.CreateAsync(notification);

            _logger.LogInformation("Saving notification: TypeId={NotificationTypeId} Title={Title} Message={Message}",
                notification.NotificationTypeId, notification.Title, notification.Message);

            await _repo.SaveAsync();

            _logger.LogInformation("Notification saved successfully: Id={NotificationId}", notification.Id);

            await _broadcaster.BroadcastNotificationAsync("", subject, notification.Message, context.CancellationToken);

            return new S_Notification.SendEmailResponse
            {
                Success = true,
                Message = "OTP email sent successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", request.Email);
            return new S_Notification.SendEmailResponse
            {
                Success = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
    }

}