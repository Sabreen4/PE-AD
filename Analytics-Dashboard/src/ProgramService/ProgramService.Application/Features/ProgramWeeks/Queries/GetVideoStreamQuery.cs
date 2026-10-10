using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Queries;

/// <summary>
/// Query to retrieve the video file stream path for a program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week.</param>
public record GetVideoStreamQuery(Int64 ProgramId, Int64 WeekId) : IRequest<VideoStreamResult>;

/// <summary>
/// Represents the result of a video stream query, containing the file path and content type.
/// </summary>
public class VideoStreamResult
{
    /// <summary>The full file path of the video on disk.</summary>
    public string FilePath { get; set; } = string.Empty;
    /// <summary>The MIME content type of the video.</summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// Handles retrieval of the video file path for streaming, with file existence validation.
/// </summary>
public class GetVideoStreamQueryHandler : IRequestHandler<GetVideoStreamQuery, VideoStreamResult>
{
    private readonly IProgramService _programService;
    private readonly IVideoStorageService _videoStorage;
    private readonly ILogger<GetVideoStreamQueryHandler> _logger;

    public GetVideoStreamQueryHandler(IProgramService programService, IVideoStorageService videoStorage, ILogger<GetVideoStreamQueryHandler> logger)
    {
        _programService = programService;
        _videoStorage = videoStorage;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of the video stream path.
    /// </summary>
    /// <returns>The video stream result containing file path and content type.</returns>
    public async Task<VideoStreamResult> Handle(GetVideoStreamQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting video stream for ProgramId: {ProgramId}, WeekId: {WeekId}", request.ProgramId, request.WeekId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null || string.IsNullOrEmpty(week.VideoPath))
        {
            _logger.LogWarning("Video not found for ProgramId: {ProgramId}, WeekId: {WeekId}", request.ProgramId, request.WeekId);
            throw new NotFoundException("Video", $"Program {request.ProgramId} Week {request.WeekId}");
        }

        string filePath = Path.IsPathRooted(week.VideoPath)
            ? week.VideoPath
            : Path.GetFullPath(week.VideoPath);

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Video file not found on disk: {FilePath}", filePath);
            throw new NotFoundException("Video", $"Program {request.ProgramId} Week {request.WeekId}");
        }

        return new VideoStreamResult
        {
            FilePath = filePath,
            ContentType = _videoStorage.GetContentType(week.VideoPath)
        };
    }
}
