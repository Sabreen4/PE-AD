namespace Shared.InterServiceHelper.Interfaces;

/// <summary>
/// Abstraction over the Notification Service gRPC client for sending
/// emails and in-app notifications.
/// </summary>
public interface INotificationGrpcClient
{
    /// <summary>Sends a registration email to the specified user.</summary>
    Task SendRegistrationEmailAsync(string email, string firstName, string loginLink);
    /// <summary>Sends an OTP email for verification purposes.</summary>
    Task SendOtpEmailAsync(string email, string otp, int expiryMinutes);
    /// <summary>Sends an in-app notification to the specified user.</summary>
    Task SendInAppNotificationAsync(Guid userId, string title, string message);
}
