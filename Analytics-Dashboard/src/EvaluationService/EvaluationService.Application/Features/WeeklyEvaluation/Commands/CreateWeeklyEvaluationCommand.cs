using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.WeeklyEvaluation;
using EvaluationService.Application.Features.Leaderboard.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Grpc.Program;
using Shared.InterServiceHelper.Interfaces;
using AttEntity = EvaluationService.Domain.Entities.Attendance;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;

namespace EvaluationService.Application.Features.WeeklyEvaluation.Commands;

/// <summary>
/// Command to create a new weekly evaluation for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="Dto">The evaluation creation data.</param>
public record CreateWeeklyEvaluationCommand(long EnrollmentId, CreateWeeklyEvaluationDto Dto) : IRequest<WeeklyEvaluationCreatedDto>;

/// <summary>
/// Handles the creation of a weekly evaluation, including validation, attendance score calculation, leaderboard recalculation, and notification.
/// </summary>
public class CreateWeeklyEvaluationCommandHandler : IRequestHandler<CreateWeeklyEvaluationCommand, WeeklyEvaluationCreatedDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateWeeklyEvaluationCommandHandler> _logger;

    public CreateWeeklyEvaluationCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        IMediator mediator,
        ILogger<CreateWeeklyEvaluationCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<WeeklyEvaluationCreatedDto> Handle(CreateWeeklyEvaluationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating weekly evaluation for EnrollmentId: {EnrollmentId}, ProgramWeekId: {ProgramWeekId}", request.EnrollmentId, request.Dto.ProgramWeekId);

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken);
        if (enrollment is null || !enrollment.Found)
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
        if (programWeek is null || !programWeek.Found)
        {
            _logger.LogWarning("ProgramWeek not found: {ProgramWeekId}", request.Dto.ProgramWeekId);
            throw new NotFoundException("ProgramWeek", request.Dto.ProgramWeekId);
        }

        if (programWeek.ProgramId != enrollment.ProgramId)
        {
            _logger.LogWarning("ProgramWeek {ProgramWeekId} does not belong to the same program as enrollment {EnrollmentId}", request.Dto.ProgramWeekId, request.EnrollmentId);
            throw new BadRequestException($"ProgramWeek {request.Dto.ProgramWeekId} does not belong to the same program as enrollment {request.EnrollmentId}.");
        }

        RefTermDto? presentStatus = await _metaDataClient.GetByKeyAsync("AttendanceStatus", "Present", cancellationToken);
        Guid presentStatusId = presentStatus is not null ? Guid.Parse(presentStatus.Id) : Guid.Empty;

        bool exists = await _evaluationService.WeeklyEvaluationExistsAsync(request.EnrollmentId, request.Dto.ProgramWeekId);

        List<AttEntity> attendanceRecords = await _evaluationService.GetAttendancesByEnrollmentAndWeekAsync(request.EnrollmentId, request.Dto.ProgramWeekId);

        int presentCount = attendanceRecords.Count(a => a.AttendanceStatusId == presentStatusId);
        int totalCount = attendanceRecords.Count;

        decimal attendanceRatio = totalCount > 0
            ? Math.Round((decimal)presentCount / totalCount, 2)
            : 0m;

        int attendanceScore = attendanceRatio > 0.75m ? 3
            : attendanceRatio >= 0.50m ? 2
            : 1;

        decimal weeklyScore = Math.Round((decimal)(attendanceScore + request.Dto.EngagementScore + request.Dto.ApplicationScore) / 3m, 2);

        if (exists)
        {
            List<WEEntity> existingEvals = await _evaluationService.GetWeeklyEvaluationsByEnrollmentAsync(request.EnrollmentId);
            WEEntity? existing = existingEvals.FirstOrDefault(w => w.ProgramWeekId == request.Dto.ProgramWeekId);
            if (existing is not null)
            {
                _logger.LogInformation("Updating existing weekly evaluation {Id}", existing.Id);
                existing.AttendanceScore = attendanceScore;
                existing.EngagementScore = request.Dto.EngagementScore;
                existing.ApplicationScore = request.Dto.ApplicationScore;
                existing.WeeklyAverage = weeklyScore;
                existing.AttendancePercentage = attendanceRatio;
                await _evaluationService.UpdateWeeklyEvaluationAsync(existing);
                await _evaluationService.SaveAsync();

                await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

                if (Guid.TryParse(enrollment.EmployeeId, out Guid employeeId))
                {
                    await _notificationClient.SendInAppNotificationAsync(
                        employeeId,
                        "Weekly Evaluation Updated",
                        $"Your weekly evaluation has been updated with a score of {weeklyScore}.");
                }

                return new WeeklyEvaluationCreatedDto { Id = existing.Id };
            }
        }

        WEEntity entity = new WEEntity
        {
            ProgramWeekId = request.Dto.ProgramWeekId,
            EmployeeAssignmentId = request.Dto.EmployeeAssignmentId,
            EnrollmentId = request.EnrollmentId,
            AttendanceScore = attendanceScore,
            EngagementScore = request.Dto.EngagementScore,
            ApplicationScore = request.Dto.ApplicationScore,
            WeeklyAverage = weeklyScore,
            AttendancePercentage = attendanceRatio
        };

        await _evaluationService.CreateWeeklyEvaluationAsync(entity);
        await _evaluationService.SaveAsync();

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        if (Guid.TryParse(enrollment.EmployeeId, out Guid notifEmployeeId))
        {
            await _notificationClient.SendInAppNotificationAsync(
                notifEmployeeId,
                "Weekly Evaluation Created",
                $"Your weekly evaluation has been created with a score of {weeklyScore}.");
        }

        return new WeeklyEvaluationCreatedDto { Id = entity.Id };
    }
}
