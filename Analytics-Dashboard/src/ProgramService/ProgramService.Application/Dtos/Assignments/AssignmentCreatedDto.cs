namespace ProgramService.Application.Dtos.Assignments;

/// <summary>
/// Data transfer object returned after an assignment is created.
/// </summary>
public class AssignmentCreatedDto
{
    public Int64 Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

