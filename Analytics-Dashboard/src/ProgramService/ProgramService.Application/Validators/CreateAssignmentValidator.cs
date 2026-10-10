using FluentValidation;
using ProgramService.Application.Dtos.Assignments;

namespace ProgramService.Application.Validators;

/// <summary>
/// Validates the data for creating a new assignment.
/// </summary>
public class CreateAssignmentValidator : AbstractValidator<CreateAssignmentDto>
{
    public CreateAssignmentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.MaxMarks).GreaterThan(0);
    }
}

/// <summary>
/// Validates the data for updating an existing assignment.
/// </summary>
public class UpdateAssignmentValidator : AbstractValidator<UpdateAssignmentDto>
{
    public UpdateAssignmentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.MaxMarks).GreaterThan(0);
    }
}

