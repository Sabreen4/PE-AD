namespace NotificationService.Infrastructure.Settings;

/// <summary>
/// Configuration settings for SMTP email delivery.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>The configuration section name used to bind these settings.</summary>
    public const string SectionName = "EmailSettings";

    /// <summary>Gets or sets the SMTP server hostname or IP address.</summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>Gets or sets the SMTP server port number.</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets the display name shown as the email sender.</summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender email address.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the SMTP password or app-specific password.</summary>
    public string Password { get; set; } = string.Empty;
}
