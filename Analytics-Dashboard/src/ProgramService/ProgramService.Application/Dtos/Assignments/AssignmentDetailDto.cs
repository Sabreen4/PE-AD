namespace ProgramService.Application.Dtos.Assignments;

/// <summary>
/// Detailed data transfer object for an assignment, including file type and metadata.
/// </summary>
public class AssignmentDetailDto
{
    public Int64 Id { get; set; }
    public Int64 ProgramId { get; set; }
    public Int64 ProgramWeekId { get; set; }
    public string? FileType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Int32 MaxMarks { get; set; }
}

