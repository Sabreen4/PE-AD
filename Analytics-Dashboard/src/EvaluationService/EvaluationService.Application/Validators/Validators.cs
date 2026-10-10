using EvaluationService.Application.Dtos.Attendance;
using EvaluationService.Application.Dtos.Feedback;
using EvaluationService.Application.Dtos.WeeklyEvaluation;
using FluentValidation;

namespace EvaluationService.Application.Validators;

/// <summary>
/// Validates <see cref="CreateAttendanceDto"/> to ensure attendance status, date, and program week are provided.
/// </summary>
public class CreateAttendanceValidator : AbstractValidator<CreateAttendanceDto>
{
    public CreateAttendanceValidator()
    {
        RuleFor(x => x.AttendanceStatus).NotEmpty().WithMessage("AttendanceStatus is required");
        RuleFor(x => x.AttendanceDate).NotEmpty().WithMessage("AttendanceDate must not be empty");
        RuleFor(x => x.ProgramWeekId).GreaterThan(0).WithMessage("ProgramWeekId must be greater than 0");
    }
}

/// <summary>
/// Validates <see cref="CreateWeeklyEvaluationDto"/> ensuring program week ID is positive and scores are within range.
/// </summary>
public class CreateWeeklyEvaluationValidator : AbstractValidator<CreateWeeklyEvaluationDto>
{
    public CreateWeeklyEvaluationValidator()
    {
        RuleFor(x => x.ProgramWeekId).GreaterThan(0);
        RuleFor(x => x.EngagementScore).InclusiveBetween(0, 3);
        RuleFor(x => x.ApplicationScore).InclusiveBetween(0, 3);
    }
}

/// <summary>
/// Validates <see cref="CreateFeedbackDto"/> ensuring employee ID, evaluation ID, and feedback type are valid.
/// </summary>
public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.WeeklyEvaluationId).GreaterThan(0);
        RuleFor(x => x.FeedbackTypeCode).NotEmpty()
            .WithMessage("FeedbackTypeCode is required.");
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

/// <summary>
/// Validates <see cref="UpdateFeedbackDto"/> ensuring feedback type code is valid and comment length is within limits.
/// </summary>
public class UpdateFeedbackValidator : AbstractValidator<UpdateFeedbackDto>
{
    public UpdateFeedbackValidator()
    {
        RuleFor(x => x.FeedbackTypeCode).NotEmpty()
            .WithMessage("FeedbackTypeCode is required.");
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}
