using FluentValidation;
using NotificationService.Application.Dtos.Notifications;

namespace NotificationService.Application.Validators;

/// <summary>
/// Validates <see cref="SendOtpEmailRequestDto"/> fields including email format, OTP code, and expiry duration.
/// </summary>
public sealed class SendOtpEmailRequestDtoValidator : AbstractValidator<SendOtpEmailRequestDto>
{
    /// <summary>
    /// Initializes validation rules for the OTP email request.
    /// </summary>
    public SendOtpEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Otp)
            .NotEmpty()
            .Length(4, 10);

        RuleFor(x => x.ExpiryMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(60);
    }
}
