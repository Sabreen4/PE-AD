using FluentValidation;
using ProgramService.Application.Dtos.Programs;

namespace ProgramService.Application.Validators;

/// <summary>
/// Validates the data for creating a new training program.
/// </summary>
public class CreateProgramValidator : AbstractValidator<CreateProgramDto>
{
    public CreateProgramValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationWeeks).InclusiveBetween(1, 52);
    }
}

/// <summary>
/// Validates the data for updating an existing training program.
/// </summary>
public class UpdateProgramValidator : AbstractValidator<UpdateProgramDto>
{
    public UpdateProgramValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationWeeks).InclusiveBetween(1, 52);
    }
}

