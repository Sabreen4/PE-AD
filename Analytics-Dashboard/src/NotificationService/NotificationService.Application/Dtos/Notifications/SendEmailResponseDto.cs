namespace NotificationService.Application.Dtos.Notifications;

/// <summary>
/// Data transfer object for the response of an email send operation.
/// </summary>
public sealed class SendEmailResponseDto
{
    /// <summary>Gets or sets a value indicating whether the operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the ID of the created notification, if applicable.</summary>
    public long? NotificationId { get; set; }
}