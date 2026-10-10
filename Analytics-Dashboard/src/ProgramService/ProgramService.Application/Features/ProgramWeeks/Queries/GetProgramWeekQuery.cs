using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Queries;

/// <summary>
/// Query to retrieve a specific program week by program and week IDs.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="WeekId">The ID of the program week.</param>
public record GetProgramWeekQuery(Int64 ProgramId, Int64 WeekId) : IRequest<ProgramWeekDetailDto>;

/// <summary>
/// Handles retrieval of a single program week.
/// </summary>
public class GetProgramWeekQueryHandler : IRequestHandler<GetProgramWeekQuery, ProgramWeekDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProgramWeekQueryHandler> _logger;

    public GetProgramWeekQueryHandler(IProgramService programService, IMapper mapper, ILogger<GetProgramWeekQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of the program week.
    /// </summary>
    /// <returns>The program week detail data.</returns>
    public async Task<ProgramWeekDetailDto> Handle(GetProgramWeekQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting program week: {WeekId} for ProgramId: {ProgramId}", request.WeekId, request.ProgramId);

        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.WeekId);
        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {WeekId}", request.WeekId);
            throw new NotFoundException("ProgramWeek", request.WeekId);
        }

        ProgramWeekDetailDto dto = _mapper.Map<ProgramWeekDetailDto>(week);
        dto.ProgramId = week.TrainingProgramId;
        return dto;
    }
}
