using FluentValidation;
using ProgramService.Application.Dtos.ProgramWeeks;

namespace ProgramService.Application.Validators;

/// <summary>
/// Validates the data for creating a new program week.
/// </summary>
public class CreateProgramWeekValidator : AbstractValidator<CreateProgramWeekDto>
{
    public CreateProgramWeekValidator()
    {
        RuleFor(x => x.WeekNumber).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// Validates the data for updating an existing program week.
/// </summary>
public class UpdateProgramWeekValidator : AbstractValidator<UpdateProgramWeekDto>
{
    public UpdateProgramWeekValidator()
    {
        RuleFor(x => x.WeekNumber).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

