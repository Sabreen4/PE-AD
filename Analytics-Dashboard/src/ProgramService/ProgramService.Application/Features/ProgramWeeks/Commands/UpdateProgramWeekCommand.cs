using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Commands;

/// <summary>
/// Command to update an existing program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week to update.</param>
/// <param name="Dto">The program week update data.</param>
public record UpdateProgramWeekCommand(Int64 ProgramId, Int64 WeekId, UpdateProgramWeekDto Dto) : IRequest<ProgramWeekDetailDto>;

/// <summary>
/// Handles the update of a program week's details.
/// </summary>
public class UpdateProgramWeekCommandHandler : IRequestHandler<UpdateProgramWeekCommand, ProgramWeekDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProgramWeekCommandHandler> _logger;

    public UpdateProgramWeekCommandHandler(IProgramService programService, IMapper mapper, ILogger<UpdateProgramWeekCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the program week update process.
    /// </summary>
    /// <returns>The updated program week detail data.</returns>
    public async Task<ProgramWeekDetailDto> Handle(UpdateProgramWeekCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating program week: {WeekId} for ProgramId: {ProgramId}", request.WeekId, request.ProgramId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        week.WeekNumber = request.Dto.WeekNumber;
        week.Title = request.Dto.Title;
        week.Description = request.Dto.Description;

        _programService.UpdateProgramWeek(week);
        await _programService.SaveAsync();

        return _mapper.Map<ProgramWeekDetailDto>(week);
    }
}
