namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for requesting an OTP email send.
/// </summary>
public sealed class SendOtpEmailRequestDto
{
    /// <summary>Gets or sets the recipient email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the OTP code to send.</summary>
    public string Otp { get; set; } = string.Empty;

    /// <summary>Gets or sets the OTP expiry duration in minutes.</summary>
    public int ExpiryMinutes { get; set; }
}