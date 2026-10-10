using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// Service for sending HTML emails via SMTP using MailKit.
/// Reads sender and server configuration from <see cref="EmailSettings"/>.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="settings">SMTP email settings.</param>
    /// <param name="logger">Logger instance.</param>
    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sends an HTML email asynchronously via SMTP using MailKit.
    /// </summary>
    /// <param name="toEmail">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The HTML body of the email.</param>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Sending email to {ToEmail} with subject {Subject}", toEmail, subject);

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogError("Sender email is not configured");
            throw new InvalidOperationException("Sender email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogError("Recipient email cannot be null or empty");
            throw new ArgumentException(
                "Recipient email cannot be null or empty.",
                nameof(toEmail)
            );
        }

        MimeMessage email = new MimeMessage();
        email.From.Add(
            new MailboxAddress(_settings.SenderName ?? "No Reply", _settings.SenderEmail)
        );
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = body };

        using SmtpClient smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _settings.SmtpServer,
            _settings.Port,
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
    }
}
