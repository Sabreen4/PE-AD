namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for a single notification response.
/// </summary>
public sealed class NotificationResponseDto
{
    /// <summary>Gets or sets the user notification ID.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the notification title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the notification message body.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the notification channel (e.g. "IN_APP").</summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the notification has been read.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets or sets the timestamp when the notification was created.</summary>
    public DateTime CreatedAt { get; set; }
}