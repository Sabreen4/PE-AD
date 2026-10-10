using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Videos;

namespace ProgramService.Infrastructure.Services;

/// <summary>
/// Service for saving, deleting, and determining content types of video files
/// stored on the local file system for program weeks.
/// </summary>
public sealed class VideoStorageService : IVideoStorageService
{
    private readonly VideoStorageOptions _options;
    private readonly ILogger<VideoStorageService> _logger;

    public VideoStorageService(IOptions<VideoStorageOptions> options, ILogger<VideoStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SaveVideoAsync(long programId, int weekNumber, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        string? ext = Path.GetExtension(fileName);
        string root = Path.GetFullPath(_options.RootPath);
        string dir = Path.Combine(root, "ProgramWeek", programId.ToString(), weekNumber.ToString());
        Directory.CreateDirectory(dir);

        string videoFileName = $"{Guid.NewGuid()}{ext}";
        string filePath = Path.Combine(dir, videoFileName);

        _logger.LogInformation("Upload started: {FilePath}", filePath);

        await using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await fileStream.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation("Upload completed: {FilePath} ({Size} bytes)", filePath, fileStream.Length);

        return filePath;
    }

    public Task DeleteFileAsync(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    public string GetContentType(string videoPath)
    {
        string? ext = Path.GetExtension(videoPath)?.ToLowerInvariant();
        return ext switch
        {
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            _ => "application/octet-stream"
        };
    }
}
