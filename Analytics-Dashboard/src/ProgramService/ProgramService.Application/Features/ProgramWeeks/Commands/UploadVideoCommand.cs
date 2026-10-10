using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Videos;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Commands;

/// <summary>
/// Command to upload a video file for a program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week.</param>
/// <param name="FileStream">The video file stream.</param>
/// <param name="FileName">The name of the video file.</param>
/// <param name="FileSize">The size of the video file in bytes.</param>
public record UploadVideoCommand(Int64 ProgramId, Int64 WeekId, Stream? FileStream, string FileName, long FileSize) : IRequest<VideoResponseDto>;

/// <summary>
/// Handles video upload with file extension and size validation, then saves to storage.
/// </summary>
public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, VideoResponseDto>
{
    private readonly IProgramService _programService;
    private readonly IVideoStorageService _videoStorage;
    private readonly VideoStorageOptions _storageOptions;
    private readonly ILogger<UploadVideoCommandHandler> _logger;

    public UploadVideoCommandHandler(
        IProgramService programService,
        IVideoStorageService videoStorage,
        IOptions<VideoStorageOptions> storageOptions,
        ILogger<UploadVideoCommandHandler> logger)
    {
        _programService = programService;
        _videoStorage = videoStorage;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Handles the video upload process.
    /// </summary>
    /// <returns>The uploaded video response data.</returns>
    public async Task<VideoResponseDto> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading video for ProgramId: {ProgramId}, WeekId: {WeekId}", request.ProgramId, request.WeekId);

        if (request.FileStream is null || request.FileSize == 0)
            throw new BadRequestException("Video file is required.");

        string ext = Path.GetExtension(request.FileName);
        if (string.IsNullOrEmpty(ext) || !_storageOptions.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new BadRequestException($"File extension '{ext}' is not allowed. Allowed: {string.Join(", ", _storageOptions.AllowedExtensions)}");

        long maxBytes = _storageOptions.MaxFileSizeMB * 1024L * 1024L;
        if (request.FileSize > maxBytes)
            throw new BadRequestException($"File size exceeds the maximum allowed size of {_storageOptions.MaxFileSizeMB} MB.");

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        // Delete existing video and thumbnail if re-uploading
        await _videoStorage.DeleteFileAsync(week.VideoPath);
        await _videoStorage.DeleteFileAsync(week.ThumbnailPath);

        // Save uploaded video stream to disk
        string videoPath = await _videoStorage.SaveVideoAsync(request.ProgramId, week.WeekNumber, request.FileStream, request.FileName, cancellationToken);

        // Update entity metadata
        week.VideoName = Path.GetFileName(request.FileName);
        week.VideoPath = videoPath;
        week.VideoSize = request.FileSize;

        await _programService.UpdateProgramWeekVideoAsync(week);
        await _programService.SaveAsync();

        return new VideoResponseDto
        {
            ProgramWeekId = request.WeekId,
            FileName = Path.GetFileName(request.FileName),
            FileSize = request.FileSize,
            Message = "Video uploaded successfully."
        };
    }
}
