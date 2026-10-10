using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Common;
using EvaluationService.Application.Dtos.Leaderboard;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.Program;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;
using LbEntity = EvaluationService.Domain.Entities.Leaderboard;
using FbEntity = EvaluationService.Domain.Entities.Feedback;
using AsEntity = EvaluationService.Domain.Entities.AssessmentEvaluation;

namespace EvaluationService.Application.Features.Leaderboard.Commands;

/// <summary>
/// Command to recalculate the leaderboard scores for all enrollments in a given program.
/// </summary>
/// <param name="ProgramId">The program identifier.</param>
public record RecalculateLeaderboardCommand(long ProgramId) : IRequest<LeaderboardListResponseDto>;

/// <summary>
/// Handles the recalculation of leaderboard scores by aggregating weekly evaluations, assessment marks, and feedback adjustments, then ranking enrollments.
/// </summary>
public class RecalculateLeaderboardCommandHandler : IRequestHandler<RecalculateLeaderboardCommand, LeaderboardListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly IMapper _mapper;
    private readonly ILogger<RecalculateLeaderboardCommandHandler> _logger;

    public RecalculateLeaderboardCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IIdentityLookupServiceClient identityClient,
        IMapper mapper,
        ILogger<RecalculateLeaderboardCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _identityClient = identityClient;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by recalculating overall scores from weekly evaluations, assessments, and feedback adjustments, then ranking enrollments.
    /// </summary>
    /// <returns>The recalculated leaderboard.</returns>
    public async Task<LeaderboardListResponseDto> Handle(RecalculateLeaderboardCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recalculating leaderboard for ProgramId: {ProgramId}", request.ProgramId);

        List<WEEntity> allEvals = await _evaluationService.GetAllWeeklyEvaluationsAsync();
        List<AsEntity> allAssessments = await _evaluationService.GetAllAssessmentEvaluationsAsync();
        List<FbEntity> allFeedbacks = await _evaluationService.GetAllFeedbacksAsync();

        Dictionary<long, EnrollmentAggregate> enrollmentData = new Dictionary<long, EnrollmentAggregate>();

        var distinctEnrollmentIds = allEvals.Select(e => e.EnrollmentId).Distinct().ToList();

        var enrollmentTasks = distinctEnrollmentIds.Select(id => _programClient.GetEnrollmentByIdAsync(id, cancellationToken));
        GetEnrollmentResponse?[] enrollmentResults = await Task.WhenAll(enrollmentTasks);

        Dictionary<long, GetEnrollmentResponse?> enrollmentMap = new Dictionary<long, GetEnrollmentResponse?>();
        for (int i = 0; i < distinctEnrollmentIds.Count; i++)
        {
            enrollmentMap[distinctEnrollmentIds[i]] = enrollmentResults[i];
        }

        foreach (WEEntity eval in allEvals)
        {
            if (!enrollmentMap.TryGetValue(eval.EnrollmentId, out GetEnrollmentResponse? enrollment) || enrollment is null || !enrollment.Found)
                continue;

            if (enrollment.ProgramId != request.ProgramId)
                continue;

            if (!enrollmentData.TryGetValue(eval.EnrollmentId, out EnrollmentAggregate? agg))
            {
                agg = new EnrollmentAggregate
                {
                    EnrollmentId = eval.EnrollmentId,
                    EmployeeId = Guid.Parse(enrollment.EmployeeId),
                    ProgramId = enrollment.ProgramId
                };
                enrollmentData[eval.EnrollmentId] = agg;
            }

            agg.TotalWeeklyScore += eval.WeeklyAverage;
            agg.WeekEvalIds.Add(eval.Id);
        }

        var uniqueAssignIds = allAssessments.Select(a => a.EmployeeAssignmentId).Distinct().ToList();
        var assignToEnrollmentMap = new Dictionary<long, long>();
        if (uniqueAssignIds.Count > 0)
        {
            var lookupTasks = uniqueAssignIds.Select(id =>
                _programClient.GetEmployeeAssignmentByIdAsync(id, cancellationToken));
            var lookupResults = await Task.WhenAll(lookupTasks);
            for (int i = 0; i < uniqueAssignIds.Count; i++)
            {
                var response = lookupResults[i];
                if (response is not null && response.Found)
                {
                    assignToEnrollmentMap[uniqueAssignIds[i]] = response.EnrollmentId;
                }
            }
        }

        foreach (var kvp in enrollmentData)
        {
            EnrollmentAggregate agg = kvp.Value;
            long enrollmentId = kvp.Key;

            agg.TotalAssessmentMarks = allAssessments
                .Where(a => assignToEnrollmentMap.TryGetValue(a.EmployeeAssignmentId, out long mappedEnrollment) && mappedEnrollment == enrollmentId)
                .Sum(a => a.Score);

            foreach (FbEntity fb in allFeedbacks)
            {
                if (fb.EmployeeId == agg.EmployeeId && agg.WeekEvalIds.Contains(fb.WeeklyEvaluationId))
                {
                    agg.TotalFeedbackAdjustments += fb.FeedbackScoreAdjustment;
                }
            }

            agg.OverallScore = Math.Round(agg.TotalWeeklyScore + agg.TotalAssessmentMarks + agg.TotalFeedbackAdjustments, 2);
        }

        List<LbEntity> existingLeaderboards = await _evaluationService.GetAllLeaderboardsAsync();
        List<LbEntity> programLeaderboards = new List<LbEntity>();

        foreach (var kvp in enrollmentData)
        {
            long enrollmentId = kvp.Key;
            decimal overallScore = kvp.Value.OverallScore;

            LbEntity? existing = existingLeaderboards.FirstOrDefault(l => l.EnrollmentId == enrollmentId);
            if (existing is not null)
            {
                existing.OverallScore = overallScore;
                _evaluationService.UpdateLeaderboard(existing);
                programLeaderboards.Add(existing);
            }
            else
            {
                LbEntity newEntry = new LbEntity
                {
                    EnrollmentId = enrollmentId,
                    OverallScore = overallScore,
                    Rank = 0
                };
                await _evaluationService.CreateLeaderboardAsync(newEntry);
                programLeaderboards.Add(newEntry);
            }
        }

        await _evaluationService.SaveAsync();

        programLeaderboards = programLeaderboards
            .OrderByDescending(l => l.OverallScore)
            .Select((l, i) =>
            {
                l.Rank = i + 1;
                return l;
            })
            .ToList();

        await _evaluationService.SaveAsync();

        List<LeaderboardEntryDto> data = _mapper.Map<List<LeaderboardEntryDto>>(programLeaderboards);

        int totalRecords = data.Count;

        var enrichTasks = data.Select(async entry =>
        {
            if (enrollmentData.TryGetValue(entry.EnrollmentId, out EnrollmentAggregate? agg))
            {
                entry.EmployeeId = agg.EmployeeId;
                var employee = await _identityClient.GetEmployeeByIdAsync(entry.EmployeeId, cancellationToken);
                entry.EmployeeName = employee?.FullName ?? string.Empty;
            }

            entry.Percentile = totalRecords > 1
                ? Math.Round((decimal)(totalRecords - entry.Rank) / (totalRecords - 1) * 100, 2)
                : 100m;
        });
        await Task.WhenAll(enrichTasks);

        PaginationMetadataDto pagination = PaginationMetadataDto.Create(1, data.Count, data.Count);

        return new LeaderboardListResponseDto { Data = data, Pagination = pagination };
    }

    /// <summary>
    /// Aggregates evaluation data for a single enrollment during leaderboard recalculation.
    /// </summary>
    private class EnrollmentAggregate
    {
        public long EnrollmentId { get; set; }
        public Guid EmployeeId { get; set; }
        public long ProgramId { get; set; }
        public decimal TotalWeeklyScore { get; set; }
        public decimal TotalAssessmentMarks { get; set; }
        public decimal TotalFeedbackAdjustments { get; set; }
        public decimal OverallScore { get; set; }
        public List<long> WeekEvalIds { get; set; } = new();
    }
}
