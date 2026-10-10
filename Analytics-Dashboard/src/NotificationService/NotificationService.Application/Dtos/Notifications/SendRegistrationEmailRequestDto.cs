namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for requesting a registration email send.
/// </summary>
public sealed class SendRegistrationEmailRequestDto
{
    /// <summary>Gets or sets the recipient email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the login link to include in the email.</summary>
    public string LoginLink { get; set; } = string.Empty;
}