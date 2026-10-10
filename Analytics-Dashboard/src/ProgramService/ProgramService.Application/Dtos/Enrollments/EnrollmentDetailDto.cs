namespace ProgramService.Application.Dtos.Enrollments;

/// <summary>
/// Detailed data transfer object for an enrollment, including employee name and status.
/// </summary>
public class EnrollmentDetailDto
{
    public Int64 Id { get; set; }
    public Int64 ProgramId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Int32 CurrentWeek { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; }
}
