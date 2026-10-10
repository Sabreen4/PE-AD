namespace ProgramService.Application.Dtos.EmployeeAssignments;

/// <summary>
/// Full detail data transfer object for an employee assignment including employee and program context.
/// </summary>
public class EmployeeAssignmentFullDetailDto
{
    public Int64 Id { get; set; }
    public Int64 AssignmentId { get; set; }
    public Int64 EnrollmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public Int64 ProgramId { get; set; }
    public Int64 ProgramWeekId { get; set; }
    public Decimal? Marks { get; set; }
    public Int32 MaxMarks { get; set; }
    public string? ReviewerComment { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; }
}
