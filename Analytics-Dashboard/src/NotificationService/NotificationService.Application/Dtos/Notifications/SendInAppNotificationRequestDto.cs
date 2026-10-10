namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for requesting an in-app notification send.
/// </summary>
public sealed class SendInAppNotificationRequestDto
{
    /// <summary>Gets or sets the target user's ID.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the notification title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the notification message body.</summary>
    public string Message { get; set; } = string.Empty;
}