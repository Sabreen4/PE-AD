using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Common;
using EvaluationService.Application.Dtos.Feedback;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.InterServiceHelper.Grpc.Program;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;
using FbEntity = EvaluationService.Domain.Entities.Feedback;

namespace EvaluationService.Application.Features.Feedback.Queries;

public record GetFeedbackByEnrollmentWeekQuery(long EnrollmentId, long ProgramWeekId, int PageNumber, int PageSize, string? Search = null) : IRequest<FeedbackListResponseDto>;

public class GetFeedbackByEnrollmentWeekQueryHandler : IRequestHandler<GetFeedbackByEnrollmentWeekQuery, FeedbackListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly ILogger<GetFeedbackByEnrollmentWeekQueryHandler> _logger;

    public GetFeedbackByEnrollmentWeekQueryHandler(
        IEvaluationService evaluationService,
        ILogger<GetFeedbackByEnrollmentWeekQueryHandler> logger)
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    public async Task<FeedbackListResponseDto> Handle(GetFeedbackByEnrollmentWeekQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting feedback for enrollment {EnrollmentId}, program week {ProgramWeekId}",
            request.EnrollmentId, request.ProgramWeekId);

        WEEntity? weeklyEval = await _evaluationService.GetWeeklyEvaluationByEnrollmentAndWeekAsync(
            request.EnrollmentId, request.ProgramWeekId);

        if (weeklyEval is null)
        {
            _logger.LogInformation("No weekly evaluation found for enrollment {EnrollmentId}, week {ProgramWeekId} — returning empty feedback",
                request.EnrollmentId, request.ProgramWeekId);
            return new FeedbackListResponseDto
            {
                Data = new List<FeedbackDto>(),
                Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, 0)
            };
        }

        _logger.LogInformation("Found weekly evaluation Id={WeeklyEvalId} for enrollment {EnrollmentId}, week {ProgramWeekId}",
            weeklyEval.Id, request.EnrollmentId, request.ProgramWeekId);

        List<FbEntity> items = await _evaluationService.GetFeedbacksByWeeklyEvaluationAsync(weeklyEval.Id);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            items = items
                .Where(f => f.Comment.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || f.FeedbackTypeName.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        int totalRecords = items.Count;

        List<FbEntity> paged = items
            .OrderBy(f => f.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<FeedbackDto> data = paged.Select(f => new FeedbackDto
        {
            FeedbackId = f.Id,
            EmployeeId = f.EmployeeId,
            ProgramId = 0,
            ProgramWeekId = weeklyEval.ProgramWeekId,
            WeeklyEvaluationId = f.WeeklyEvaluationId,
            FeedbackTypeId = f.FeedbackTypeId,
            FeedbackTypeCode = f.FeedbackTypeCode,
            FeedbackTypeName = f.FeedbackTypeName,
            FeedbackScoreAdjustment = f.FeedbackScoreAdjustment,
            FeedbackComment = f.Comment,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        }).ToList();

        return new FeedbackListResponseDto
        {
            Data = data,
            Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, totalRecords)
        };
    }
}
