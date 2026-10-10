namespace ProgramService.Application.Dtos.Assignments;

/// <summary>
/// Data transfer object for updating an existing assignment.
/// </summary>
public class UpdateAssignmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Int32 MaxMarks { get; set; }
    public string? FileType { get; set; }
    public string? FilePath { get; set; }
}

