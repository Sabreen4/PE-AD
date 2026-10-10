namespace ProgramService.Application.Dtos.ProgramWeeks;

/// <summary>
/// Data transfer object for a program week.
/// </summary>
public class ProgramWeekDto
{
    public Int64 Id { get; set; }
    public Int32 WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public string? VideoName { get; set; }
    public long? VideoSize { get; set; }
    public TimeSpan? VideoDuration { get; set; }
    public string? ThumbnailPath { get; set; }
}

