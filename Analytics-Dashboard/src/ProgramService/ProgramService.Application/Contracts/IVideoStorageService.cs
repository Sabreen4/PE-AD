namespace ProgramService.Application.Contracts;

/// <summary>
/// Service interface for storing, deleting, and determining content types of video files.
/// </summary>
public interface IVideoStorageService
{
    /// <summary>
    /// Saves a video file to the storage location for the specified program week.
    /// </summary>
    /// <returns>The full file path of the saved video.</returns>
    Task<string> SaveVideoAsync(long programId, int weekNumber, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a file at the given path if it exists.
    /// </summary>
    Task DeleteFileAsync(string? filePath);
    /// <summary>
    /// Gets the MIME content type for a video file based on its extension.
    /// </summary>
    /// <returns>The MIME content type string.</returns>
    string GetContentType(string videoPath);
}
