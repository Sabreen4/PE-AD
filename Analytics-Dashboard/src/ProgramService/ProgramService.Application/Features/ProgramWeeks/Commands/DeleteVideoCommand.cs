using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Commands;

/// <summary>
/// Command to delete the video associated with a program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week.</param>
public record DeleteVideoCommand(Int64 ProgramId, Int64 WeekId) : IRequest;

/// <summary>
/// Handles the deletion of a video file and associated metadata from a program week.
/// </summary>
public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand>
{
    private readonly IProgramService _programService;
    private readonly IVideoStorageService _videoStorage;
    private readonly ILogger<DeleteVideoCommandHandler> _logger;

    public DeleteVideoCommandHandler(IProgramService programService, IVideoStorageService videoStorage, ILogger<DeleteVideoCommandHandler> logger)
    {
        _programService = programService;
        _videoStorage = videoStorage;
        _logger = logger;
    }

    /// <summary>
    /// Handles the video deletion process.
    /// </summary>
    public async Task Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting video for ProgramId: {ProgramId}, WeekId: {WeekId}", request.ProgramId, request.WeekId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        await _videoStorage.DeleteFileAsync(week.VideoPath);
        await _videoStorage.DeleteFileAsync(week.ThumbnailPath);

        week.VideoName = null;
        week.VideoPath = null;
        week.VideoSize = null;
        week.VideoDuration = null;
        week.ThumbnailPath = null;

        await _programService.UpdateProgramWeekVideoAsync(week);
        await _programService.SaveAsync();
    }
}
