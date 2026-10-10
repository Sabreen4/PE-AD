namespace ProgramService.Application.Dtos.EmployeeAssignments;

/// <summary>
/// Data transfer object for an employee assignment.
/// </summary>
public class EmployeeAssignmentDto
{
    public Int64 Id { get; set; }
    public Int64 EnrollmentId { get; set; }
    public string? Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Int64 FileSize { get; set; }
    public Decimal? Marks { get; set; }
}

