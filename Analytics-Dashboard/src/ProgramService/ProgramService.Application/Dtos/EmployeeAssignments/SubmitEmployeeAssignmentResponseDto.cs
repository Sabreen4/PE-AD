namespace ProgramService.Application.Dtos.EmployeeAssignments;

public class SubmitEmployeeAssignmentResponseDto
{
    public Int64 Id { get; set; }
    public Int64 AssignmentId { get; set; }
    public Int64 EnrollmentId { get; set; }
    public string? Status { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Int64 FileSize { get; set; }
}
