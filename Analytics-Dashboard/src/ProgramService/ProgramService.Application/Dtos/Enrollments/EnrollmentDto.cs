namespace ProgramService.Application.Dtos.Enrollments;

/// <summary>
/// Data transfer object for an enrollment.
/// </summary>
public class EnrollmentDto
{
    public Int64 Id { get; set; }
    public Int64 ProgramId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Int32 CurrentWeek { get; set; }
    public string? Status { get; set; }
}
