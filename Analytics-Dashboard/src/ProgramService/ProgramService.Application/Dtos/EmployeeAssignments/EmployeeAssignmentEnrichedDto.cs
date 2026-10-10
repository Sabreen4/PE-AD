namespace ProgramService.Application.Dtos.EmployeeAssignments;

public class EmployeeAssignmentEnrichedDto
{
    public long EmployeeAssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public long ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public long ProgramWeekId { get; set; }
    public string ProgramWeekName { get; set; } = string.Empty;
    public long AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public long EnrollmentId { get; set; }
    public string? Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public decimal? Marks { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}
