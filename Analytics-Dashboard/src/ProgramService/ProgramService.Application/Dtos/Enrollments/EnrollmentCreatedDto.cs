namespace ProgramService.Application.Dtos.Enrollments;

/// <summary>
/// Data transfer object returned after an enrollment is created.
/// </summary>
public class EnrollmentCreatedDto
{
    public Int64 Id { get; set; }
    public Guid EmployeeId { get; set; }
}

