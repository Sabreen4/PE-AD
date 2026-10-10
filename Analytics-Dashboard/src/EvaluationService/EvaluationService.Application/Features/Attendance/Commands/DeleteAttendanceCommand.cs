using EvaluationService.Application.Contracts;
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
/// Command to delete an attendance record for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="AttendanceId">The attendance record identifier.</param>
public record DeleteAttendanceCommand(long EnrollmentId, long AttendanceId) : IRequest<Unit>;

/// <summary>
/// Handles the deletion of an attendance record, including weekly evaluation recalculation.
/// </summary>
public class DeleteAttendanceCommandHandler : IRequestHandler<DeleteAttendanceCommand, Unit>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteAttendanceCommandHandler> _logger;

    public DeleteAttendanceCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        IMediator mediator,
        ILogger<DeleteAttendanceCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting attendance {AttendanceId} for enrollment {EnrollmentId}", request.AttendanceId, request.EnrollmentId);

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

        long programWeekId = existing.ProgramWeekId;

        await _evaluationService.DeleteAttendanceAsync(existing);
        await _evaluationService.SaveAsync();

        RefTermDto? presentStatus = await _metaDataClient.GetByKeyAsync("AttendanceStatus", "Present", cancellationToken);
        if (presentStatus is not null)
        {
            Guid presentStatusId = Guid.Parse(presentStatus.Id);
            await _evaluationService.RecalculateAttendanceScoreAsync(request.EnrollmentId, programWeekId, presentStatusId);
            await _evaluationService.SaveAsync();
        }

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        return Unit.Value;
    }
}
