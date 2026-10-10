namespace ProgramService.Application.Dtos.EmployeeAssignments;

/// <summary>
/// Data transfer object for evaluating an employee assignment with marks and comments.
/// </summary>
public class EvaluateEmployeeAssignmentDto
{
    public Decimal Marks { get; set; }
    public string? ReviewerComment { get; set; }
}
