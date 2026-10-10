using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Commands;

/// <summary>
/// Command to create a new program week for a training program.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="Dto">The program week creation data.</param>
public record CreateProgramWeekCommand(Int64 ProgramId, CreateProgramWeekDto Dto) : IRequest<ProgramWeekCreatedDto>;

/// <summary>
/// Handles the creation of a program week with duplicate week number validation.
/// </summary>
public class CreateProgramWeekCommandHandler : IRequestHandler<CreateProgramWeekCommand, ProgramWeekCreatedDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProgramWeekCommandHandler> _logger;

    public CreateProgramWeekCommandHandler(IProgramService programService, IMapper mapper, ILogger<CreateProgramWeekCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the program week creation process.
    /// </summary>
    /// <returns>The created program week data.</returns>
    public async Task<ProgramWeekCreatedDto> Handle(CreateProgramWeekCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating program week for ProgramId: {ProgramId}, WeekNumber: {WeekNumber}", request.ProgramId, request.Dto.WeekNumber);

        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
        if (program is null)
        {
            _logger.LogWarning("Program not found: {ProgramId}", request.ProgramId);
            throw new NotFoundException("Program", request.ProgramId);
        }

        Boolean weekExists = await _programService.ProgramWeekExistsAsync(request.ProgramId, request.Dto.WeekNumber);
        if (weekExists)
        {
            _logger.LogWarning("Week {WeekNumber} already exists for ProgramId: {ProgramId}", request.Dto.WeekNumber, request.ProgramId);
            throw new ConflictException($"Week {request.Dto.WeekNumber} already exists for this program.");
        }

        ProgramWeek week = new ProgramWeek
        {
            TrainingProgramId = request.ProgramId,
            WeekNumber = request.Dto.WeekNumber,
            Title = request.Dto.Title,
            Description = request.Dto.Description
        };

        await _programService.CreateProgramWeekAsync(week);
        await _programService.SaveAsync();

        return new ProgramWeekCreatedDto
        {
            Id = week.Id,
            Title = week.Title
        };
    }
}
