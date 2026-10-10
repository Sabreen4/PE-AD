using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Commands;

public record DeleteProgramWeekCommand(Int64 ProgramId, Int64 WeekId) : IRequest;

public class DeleteProgramWeekCommandHandler : IRequestHandler<DeleteProgramWeekCommand>
{
    private readonly IProgramService _programService;
    private readonly ILogger<DeleteProgramWeekCommandHandler> _logger;

    public DeleteProgramWeekCommandHandler(IProgramService programService, ILogger<DeleteProgramWeekCommandHandler> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    public async Task Handle(DeleteProgramWeekCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting program week: {WeekId} for ProgramId: {ProgramId}", request.WeekId, request.ProgramId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("Program week not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        await _programService.DeleteProgramWeekAsync(week);
        await _programService.SaveAsync();

        _logger.LogInformation("Program week deleted: {WeekId}", request.WeekId);
    }
}