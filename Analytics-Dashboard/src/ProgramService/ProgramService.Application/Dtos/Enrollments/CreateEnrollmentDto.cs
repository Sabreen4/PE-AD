namespace ProgramService.Application.Dtos.Enrollments;

/// <summary>
/// Data transfer object for creating a new enrollment.
/// </summary>
public class CreateEnrollmentDto
{
    public Guid EmployeeId { get; set; }
}

