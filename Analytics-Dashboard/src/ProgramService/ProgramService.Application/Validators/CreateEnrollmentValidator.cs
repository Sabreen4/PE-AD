using FluentValidation;
using ProgramService.Application.Dtos.Enrollments;

namespace ProgramService.Application.Validators;

/// <summary>
/// Validates the data for creating a new enrollment.
/// </summary>
public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentDto>
{
}

