using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Attendance;
using EvaluationService.Application.Features.Leaderboard.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Grpc.Program;
using Shared.InterServiceHelper.Interfaces;
using AttEntity = EvaluationService.Domain.Entities.Attendance;

namespace EvaluationService.Application.Features.Attendance.Commands;

/// <summary>
/// Command to create a new attendance record for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="Dto">The attendance creation data.</param>
public record CreateAttendanceCommand(long EnrollmentId, CreateAttendanceDto Dto) : IRequest<AttendanceCreatedDto>;

/// <summary>
/// Handles the creation of an attendance record, including validation of enrollment, program week, date uniqueness, and leaderboard recalculation.
/// </summary>
public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, AttendanceCreatedDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAttendanceCommandHandler> _logger;

    public CreateAttendanceCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        IMediator mediator,
        IMapper mapper,
        ILogger<CreateAttendanceCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by validating enrollment, program week, and date uniqueness, then persisting the attendance and recalculating the leaderboard.
    /// </summary>
    /// <returns>The created attendance identifier.</returns>
    public async Task<AttendanceCreatedDto> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating attendance for EnrollmentId: {EnrollmentId}, ProgramWeekId: {ProgramWeekId}", request.EnrollmentId, request.Dto.ProgramWeekId);

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken: cancellationToken);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
            throw new NotFoundException("Enrollment", request.EnrollmentId);
        }

        if (!enrollment.IsActive)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} is not active", request.EnrollmentId);
            throw new BadRequestException($"Enrollment {request.EnrollmentId} is not active.");
        }

        GetProgramWeekResponse? programWeek = await _programClient.GetProgramWeekByIdAsync(request.Dto.ProgramWeekId, cancellationToken: cancellationToken);
        if (programWeek is null)
        {
            _logger.LogWarning("ProgramWeek not found: {ProgramWeekId}", request.Dto.ProgramWeekId);
            throw new NotFoundException("ProgramWeek", request.Dto.ProgramWeekId);
        }

        if (programWeek.ProgramId != enrollment.ProgramId)
        {
            _logger.LogWarning("ProgramWeek {ProgramWeekId} does not belong to Program {ProgramId}", request.Dto.ProgramWeekId, enrollment.ProgramId);
            throw new BadRequestException($"ProgramWeek {request.Dto.ProgramWeekId} does not belong to Program {enrollment.ProgramId}.");
        }

        bool dateExists = await _evaluationService.AttendanceExistsForDateAsync(request.EnrollmentId, request.Dto.ProgramWeekId, request.Dto.AttendanceDate);
        if (dateExists)
        {
            _logger.LogWarning("Attendance already exists for EnrollmentId: {EnrollmentId}, ProgramWeekId: {ProgramWeekId}, Date: {Date}", request.EnrollmentId, request.Dto.ProgramWeekId, request.Dto.AttendanceDate);
            throw new ConflictException("An attendance record already exists for this enrollment, program week, and date.");
        }

        RefTermDto? statusRefTerm = await _metaDataClient.GetByKeyAsync("AttendanceStatus", request.Dto.AttendanceStatus, cancellationToken);
        if (statusRefTerm is null)
        {
            _logger.LogWarning("Invalid AttendanceStatus: {AttendanceStatus}", request.Dto.AttendanceStatus);
            throw new BadRequestException($"'{request.Dto.AttendanceStatus}' is not a valid AttendanceStatus reference term key.");
        }

        AttEntity entity = _mapper.Map<AttEntity>(request.Dto);
        entity.EnrollmentId = request.EnrollmentId;
        entity.AttendanceStatusId = Guid.Parse(statusRefTerm.Id);

        await _evaluationService.CreateAttendanceAsync(entity);
        await _evaluationService.SaveAsync();

        RefTermDto? presentStatus = await _metaDataClient.GetByKeyAsync("AttendanceStatus", "Present", cancellationToken);
        if (presentStatus is not null)
        {
            Guid presentStatusId = Guid.Parse(presentStatus.Id);
            await _evaluationService.RecalculateAttendanceScoreAsync(request.EnrollmentId, request.Dto.ProgramWeekId, presentStatusId);
            await _evaluationService.SaveAsync();
        }

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        return new AttendanceCreatedDto { Id = entity.Id };
    }
}
