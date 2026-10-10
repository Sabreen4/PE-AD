using EvaluationService.Application.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;
using AttEntity = EvaluationService.Domain.Entities.Attendance;

namespace EvaluationService.Application.Features.WeeklyEvaluation.Queries;

/// <summary>
/// Query to calculate the attendance score (out of 3) for a given enrollment and program week.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="ProgramWeekId">The program week identifier.</param>
public record GetAttendanceScoreQuery(long EnrollmentId, long ProgramWeekId) : IRequest<int>;

/// <summary>
/// Handles the <see cref="GetAttendanceScoreQuery"/> by fetching daily attendance records and calculating the score.
/// </summary>
public class GetAttendanceScoreQueryHandler : IRequestHandler<GetAttendanceScoreQuery, int>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<GetAttendanceScoreQueryHandler> _logger;

    public GetAttendanceScoreQueryHandler(
        IEvaluationService evaluationService,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<GetAttendanceScoreQueryHandler> logger)
    {
        _evaluationService = evaluationService;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    public async Task<int> Handle(GetAttendanceScoreQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating attendance score for EnrollmentId: {EnrollmentId}, ProgramWeekId: {ProgramWeekId}",
            request.EnrollmentId, request.ProgramWeekId);

        RefTermDto? presentStatus = await _metaDataClient.GetByKeyAsync("AttendanceStatus", "Present", cancellationToken);
        Guid presentStatusId = presentStatus is not null ? Guid.Parse(presentStatus.Id) : Guid.Empty;

        List<AttEntity> attendanceRecords = await _evaluationService.GetAttendancesByEnrollmentAndWeekAsync(
            request.EnrollmentId, request.ProgramWeekId);

        _logger.LogInformation("Found {Count} attendance records for enrollment {EnrollmentId}, week {ProgramWeekId}",
            attendanceRecords.Count, request.EnrollmentId, request.ProgramWeekId);

        int presentCount = attendanceRecords.Count(a => a.AttendanceStatusId == presentStatusId);
        int totalCount = attendanceRecords.Count;

        if (totalCount == 0)
        {
            _logger.LogInformation("No attendance records found; returning score 0");
            return 0;
        }

        decimal attendanceRatio = Math.Round((decimal)presentCount / totalCount, 2);

        int score = attendanceRatio > 0.75m ? 3
            : attendanceRatio >= 0.50m ? 2
            : 1;

        _logger.LogInformation("Attendance score calculated: {PresentCount}/{TotalCount} = {Ratio} -> {Score}",
            presentCount, totalCount, attendanceRatio, score);

        return score;
    }
}
