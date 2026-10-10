namespace ProgramService.Application.Dtos.EmployeeAssignments;

/// <summary>
/// Detailed data transfer object for an employee assignment, including marks and reviewer comments.
/// </summary>
public class EmployeeAssignmentDetailDto
{
    public Int64 Id { get; set; }
    public Int64 AssignmentId { get; set; }
    public Int64 EnrollmentId { get; set; }
    public string? Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Int64 FileSize { get; set; }
    public Decimal? Marks { get; set; }
    public string? ReviewerComment { get; set; }
}

