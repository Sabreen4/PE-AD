using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Videos;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Queries;

/// <summary>
/// Query to retrieve video metadata for a program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week.</param>
public record GetVideoMetadataQuery(Int64 ProgramId, Int64 WeekId) : IRequest<VideoMetadataDto>;

/// <summary>
/// Handles retrieval of video metadata from a program week entity.
/// </summary>
public class GetVideoMetadataQueryHandler : IRequestHandler<GetVideoMetadataQuery, VideoMetadataDto>
{
    private readonly IProgramService _programService;
    private readonly ILogger<GetVideoMetadataQueryHandler> _logger;

    public GetVideoMetadataQueryHandler(IProgramService programService, ILogger<GetVideoMetadataQueryHandler> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of video metadata.
    /// </summary>
    /// <returns>The video metadata.</returns>
    public async Task<VideoMetadataDto> Handle(GetVideoMetadataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting video metadata for ProgramId: {ProgramId}, WeekId: {WeekId}", request.ProgramId, request.WeekId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        return new VideoMetadataDto
        {
            VideoName = week.VideoName,
            VideoSize = week.VideoSize,
            VideoDuration = week.VideoDuration,
            ThumbnailPath = week.ThumbnailPath,
            WeekNumber = week.WeekNumber
        };
    }
}
