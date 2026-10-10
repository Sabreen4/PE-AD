namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for the response of sending an in-app notification.
/// </summary>
public sealed class SendInAppNotificationResponseDto
{
    /// <summary>Gets or sets a value indicating whether the operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;
}