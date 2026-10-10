using FluentValidation;
using NotificationService.Application.Dtos.Notifications;

namespace NotificationService.Application.Validators;

/// <summary>
/// Validates <see cref="SendRegistrationEmailRequestDto"/> fields including email format, first name, and login link.
/// </summary>
public sealed class SendRegistrationEmailRequestDtoValidator : AbstractValidator<SendRegistrationEmailRequestDto>
{
    /// <summary>
    /// Initializes validation rules for the registration email request.
    /// </summary>
    public SendRegistrationEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LoginLink)
            .NotEmpty()
            .MaximumLength(500);
    }
}
