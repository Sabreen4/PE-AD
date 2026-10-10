namespace ProgramService.Application.Dtos.Videos;

/// <summary>
/// Configuration options for video file storage.
/// </summary>
public class VideoStorageOptions
{
    public const string SectionName = "VideoStorage";

    public string RootPath { get; set; } = "Uploads/Videos";
    public int MaxFileSizeMB { get; set; } = 500;
    public List<string> AllowedExtensions { get; set; } = new() { ".mp4", ".mov", ".avi", ".mkv" };
}
