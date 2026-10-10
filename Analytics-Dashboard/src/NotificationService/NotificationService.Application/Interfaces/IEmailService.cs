namespace NotificationService.Application.Interfaces;

/// <summary>
/// Defines a service for sending emails asynchronously.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to the specified recipient with the given subject and body.
    /// </summary>
    /// <param name="toEmail">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The HTML email body.</param>
    Task SendEmailAsync(string toEmail, string subject, string body);
}
