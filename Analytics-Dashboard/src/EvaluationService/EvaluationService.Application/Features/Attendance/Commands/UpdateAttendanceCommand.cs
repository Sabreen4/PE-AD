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
/// Command to update an existing attendance record for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="AttendanceId">The attendance record identifier.</param>
/// <param name="Dto">The attendance update data.</param>
public record UpdateAttendanceCommand(long EnrollmentId, long AttendanceId, UpdateAttendanceDto Dto) : IRequest<AttendanceDto>;

/// <summary>
/// Handles the update of an attendance record, including validation and weekly evaluation recalculation.
/// </summary>
public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, AttendanceDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAttendanceCommandHandler> _logger;

    public UpdateAttendanceCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        IMediator mediator,
        IMapper mapper,
        ILogger<UpdateAttendanceCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AttendanceDto> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating attendance {AttendanceId} for enrollment {EnrollmentId}", request.AttendanceId, request.EnrollmentId);

        AttEntity? existing = await _evaluationService.GetAttendanceByIdAsync(request.AttendanceId);
        if (existing is null)
        {
            _logger.LogWarning("Attendance not found: {AttendanceId}", request.AttendanceId);
            throw new NotFoundException("Attendance", request.AttendanceId);
        }

        if (existing.EnrollmentId != request.EnrollmentId)
        {
            _logger.LogWarning("Attendance {AttendanceId} does not belong to enrollment {EnrollmentId}", request.AttendanceId, request.EnrollmentId);
            throw new BadRequestException($"Attendance {request.AttendanceId} does not belong to enrollment {request.EnrollmentId}.");
        }

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken: cancellationToken);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
            throw new NotFoundException("Enrollment", request.EnrollmentId);
        }

        if (request.Dto.AttendanceDate.HasValue)
            existing.AttendanceDate = request.Dto.AttendanceDate.Value;

        if (!string.IsNullOrWhiteSpace(request.Dto.AttendanceStatus))
        {
            RefTermDto? statusRefTerm = await _metaDataClient.GetByKeyAsync("AttendanceStatus", request.Dto.AttendanceStatus, cancellationToken);
            if (statusRefTerm is null)
            {
                _logger.LogWarning("Invalid AttendanceStatus: {AttendanceStatus}", request.Dto.AttendanceStatus);
                throw new BadRequestException($"'{request.Dto.AttendanceStatus}' is not a valid AttendanceStatus reference term key.");
            }
            existing.AttendanceStatusId = Guid.Parse(statusRefTerm.Id);
        }

        await _evaluationService.UpdateAttendanceAsync(existing);
        await _evaluationService.SaveAsync();

        RefTermDto? presentStatus = await _metaDataClient.GetByKeyAsync("AttendanceStatus", "Present", cancellationToken);
        if (presentStatus is not null)
        {
            Guid presentStatusId = Guid.Parse(presentStatus.Id);
            await _evaluationService.RecalculateAttendanceScoreAsync(request.EnrollmentId, existing.ProgramWeekId, presentStatusId);
            await _evaluationService.SaveAsync();
        }

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        AttendanceDto result = _mapper.Map<AttendanceDto>(existing);
        return result;
    }
}
