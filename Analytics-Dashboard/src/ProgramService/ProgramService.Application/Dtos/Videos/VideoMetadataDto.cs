namespace ProgramService.Application.Dtos.Videos;

/// <summary>
/// Data transfer object for video metadata associated with a program week.
/// </summary>
public class VideoMetadataDto
{
    public string? VideoName { get; set; }
    public long? VideoSize { get; set; }
    public TimeSpan? VideoDuration { get; set; }
    public string? ThumbnailPath { get; set; }
    public int WeekNumber { get; set; }
}
