using FluentValidation;
using IdentityService.Application.Dtos.Users;

namespace IdentityService.Application.Validators;

/// <summary>
/// Validates the <see cref="CreateUserRequestDto"/> before processing user creation.
/// </summary>
public sealed class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserRequestDtoValidator"/> class.
    /// </summary>
    public CreateUserRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Length(10)
            .Must(x => x.All(Char.IsDigit))
            .WithMessage("Phone number must be exactly 10 digits.");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0)
            .WithMessage("Organization is required.");

        RuleFor(x => x.RoleType)
            .NotEmpty()
            .WithMessage("Role type is required.");

        RuleFor(x => x.Gender)
            .MaximumLength(50);
    }
}
