namespace ProgramService.Application.Dtos.EmployeeAssignments;

/// <summary>
/// Data transfer object returned after an employee assignment is created.
/// </summary>
public class CreateEmployeeAssignmentResponseDto
{
    public Int64 Id { get; set; }
    public Int64 AssignmentId { get; set; }
    public Int64 EnrollmentId { get; set; }
    public string? Status { get; set; }
}
