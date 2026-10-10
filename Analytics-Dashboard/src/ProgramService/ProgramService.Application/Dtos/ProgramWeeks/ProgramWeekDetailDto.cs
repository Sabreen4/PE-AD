namespace ProgramService.Application.Dtos.ProgramWeeks;

/// <summary>
/// Detailed data transfer object for a program week, including video metadata.
/// </summary>
public class ProgramWeekDetailDto
{
    public Int64 ProgramId { get; set; }
    public Int32 WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public string? VideoName { get; set; }
    public long? VideoSize { get; set; }
    public TimeSpan? VideoDuration { get; set; }
    public string? ThumbnailPath { get; set; }
}

