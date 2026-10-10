using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.Notification;
using Shared.InterServiceHelper.Interfaces;

namespace Shared.InterServiceHelper.Services;

/// <summary>
/// Wraps the generated <see cref="NotificationService.NotificationServiceClient"/>.
/// This is the only class allowed to reference the generated gRPC client directly;
/// every other consumer depends on <see cref="INotificationGrpcClient"/>.
/// </summary>
public sealed class NotificationGrpcClient : INotificationGrpcClient
{
    private readonly NotificationService.NotificationServiceClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NotificationGrpcClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationGrpcClient"/> class.
    /// </summary>
    /// <param name="client">The generated Notification Service gRPC client.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for forwarding auth headers.</param>
    /// <param name="logger">The logger instance.</param>
    public NotificationGrpcClient(
        NotificationService.NotificationServiceClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<NotificationGrpcClient> logger)
    {
        _client = client;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Builds gRPC metadata headers including the current Authorization header
    /// for forwarding to downstream services.
    /// </summary>
    private Metadata GetMetadataWithAuth()
    {
        Metadata headers = new Metadata();
        string? authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
            headers.Add("authorization", authHeader);
        return headers;
    }

    /// <summary>
    /// Sends a registration email to the specified user.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="firstName">The recipient's first name.</param>
    /// <param name="loginLink">The login link to include in the email.</param>
    public async Task SendRegistrationEmailAsync(string email, string firstName, string loginLink)
    {
        _logger.LogInformation("Sending registration email to {Email}", email);

        try
        {
            SendEmailResponse response = await _client.SendRegistrationEmailAsync(
                new SendRegistrationEmailRequest
                {
                    Email = email,
                    FirstName = firstName,
                    LoginLink = loginLink
                },
                GetMetadataWithAuth());

            if (!response.Success)
                _logger.LogWarning("Notification Service returned failure for registration email to {Email}: {Message}", email, response.Message);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to send registration email to {Email}", email);
        }
    }

    /// <summary>
    /// Sends an OTP email to the specified user for verification.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="otp">The one-time password to send.</param>
    /// <param name="expiryMinutes">The number of minutes before the OTP expires.</param>
    public async Task SendOtpEmailAsync(string email, string otp, int expiryMinutes)
    {
        _logger.LogInformation("Sending OTP email to {Email}, expires in {ExpiryMinutes} minutes", email, expiryMinutes);

        try
        {
            SendEmailResponse response = await _client.SendOtpEmailAsync(
                new SendOtpEmailRequest
                {
                    Email = email,
                    Otp = otp,
                    ExpiryMinutes = expiryMinutes
                },
                GetMetadataWithAuth());

            if (!response.Success)
                _logger.LogWarning("Notification Service returned failure for OTP email to {Email}: {Message}", email, response.Message);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to send OTP email to {Email}", email);
        }
    }

    /// <summary>
    /// Sends an in-app notification to the specified user.
    /// </summary>
    /// <param name="userId">The recipient user's unique identifier.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message body.</param>
    public async Task SendInAppNotificationAsync(Guid userId, string title, string message)
    {
        _logger.LogInformation("Sending in-app notification to UserId: {UserId}, Title: {Title}", userId, title);

        try
        {
            SendInAppNotificationResponse response = await _client.SendInAppNotificationAsync(
                new SendInAppNotificationRequest
                {
                    UserId = userId.ToString(),
                    Title = title,
                    Message = message
                },
                GetMetadataWithAuth());

            if (!response.Success)
                _logger.LogWarning("Notification Service returned failure for in-app notification to {UserId}: {Message}", userId, response.Message);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to send in-app notification to UserId: {UserId}", userId);
        }
    }
}
