namespace ProgramService.Application.Dtos.Videos;

/// <summary>
/// Data transfer object returned after a video upload operation.
/// </summary>
public class VideoResponseDto
{
    public long ProgramWeekId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public TimeSpan? VideoDuration { get; set; }
    public string Message { get; set; } = "Video uploaded successfully.";
}
