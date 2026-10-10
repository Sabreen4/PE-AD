using FluentValidation;
using IdentityService.Application.Dtos.Organizations;

namespace IdentityService.Application.Validators;

/// <summary>
/// Validates the <see cref="CreateOrganizationRequestDto"/> before processing organization creation.
/// </summary>
public sealed class CreateOrganizationRequestDtoValidator : AbstractValidator<CreateOrganizationRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrganizationRequestDtoValidator"/> class.
    /// </summary>
    public CreateOrganizationRequestDtoValidator()
    {
        RuleFor(x => x.OrganizationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.OrganizationEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.OrganizationPhone)
            .NotEmpty()
            .Length(10)
            .Must(x => x.All(char.IsDigit))
            .WithMessage("Phone number must be exactly 10 digits.");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminFirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminLastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.AdminPhone)
            .NotEmpty()
            .Length(10)
            .Must(x => x.All(char.IsDigit))
            .WithMessage("Admin phone number must be exactly 10 digits.");

        RuleFor(x => x.AdminGender)
            .MaximumLength(50);
    }
}
