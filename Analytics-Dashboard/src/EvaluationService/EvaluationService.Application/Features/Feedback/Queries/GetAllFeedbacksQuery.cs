using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Common;
using EvaluationService.Application.Dtos.Feedback;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.InterServiceHelper.Grpc.Program;
using FbEntity = EvaluationService.Domain.Entities.Feedback;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;

namespace EvaluationService.Application.Features.Feedback.Queries;

/// <summary>
/// Query to retrieve a paginated list of feedback entries for a given weekly evaluation.
/// </summary>
/// <param name="WeeklyEvaluationId">The weekly evaluation identifier.</param>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
public record GetAllFeedbacksQuery(long WeeklyEvaluationId, int PageNumber, int PageSize, string? Search = null) : IRequest<FeedbackListResponseDto>;

/// <summary>
/// Handles the <see cref="GetAllFeedbacksQuery"/> by fetching feedback entries for a weekly evaluation and mapping them to DTOs with program context.
/// </summary>
public class GetAllFeedbacksQueryHandler : IRequestHandler<GetAllFeedbacksQuery, FeedbackListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFeedbacksQueryHandler> _logger;
    private readonly IUserContextService _userContext;

    public GetAllFeedbacksQueryHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMapper mapper,
        ILogger<GetAllFeedbacksQueryHandler> logger,
        IUserContextService userContext)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _mapper = mapper;
        _logger = logger;
        _userContext = userContext;
    }

    /// <summary>
    /// Handles the query by fetching feedback entries for a weekly evaluation, enriching them with program context, and paginating.
    /// </summary>
    /// <returns>A paginated list of feedback entries.</returns>
    public async Task<FeedbackListResponseDto> Handle(GetAllFeedbacksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== FEEDBACK DEBUG: GetAllFeedbacksQueryHandler START ===");
        _logger.LogInformation("FEEDBACK DEBUG: WeeklyEvaluationId received = {WeeklyEvaluationId}", request.WeeklyEvaluationId);
        _logger.LogInformation("FEEDBACK DEBUG: UserContext UserId = {UserId}, Roles = {Roles}",
            _userContext.UserId, string.Join(",", _userContext.Roles));

        WEEntity? weeklyEval = await _evaluationService.GetWeeklyEvaluationByIdAsync(request.WeeklyEvaluationId);
        _logger.LogInformation("FEEDBACK DEBUG: WeeklyEvaluation lookup result = {Found}", weeklyEval is not null ? $"Found (Id={weeklyEval.Id}, ProgramWeekId={weeklyEval.ProgramWeekId})" : "NULL");
        if (weeklyEval is null)
        {
            _logger.LogWarning("FEEDBACK DEBUG: WeeklyEvaluation {WeeklyEvaluationId} NOT FOUND — returning empty", request.WeeklyEvaluationId);
            return new FeedbackListResponseDto
            {
                Data = new List<FeedbackDto>(),
                Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, 0)
            };
        }

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(weeklyEval.EnrollmentId, cancellationToken);

        List<FbEntity> items = await _evaluationService.GetFeedbacksByWeeklyEvaluationAsync(request.WeeklyEvaluationId);
        _logger.LogInformation("FEEDBACK DEBUG: Raw feedback records from DB = {Count}", items.Count);
        if (items.Count > 0)
        {
            var fbIds = string.Join(", ", items.Select(f => $"Id={f.Id}, EmployeeId={f.EmployeeId}"));
            _logger.LogInformation("FEEDBACK DEBUG: Raw feedback list: [{FbIds}]", fbIds);
        }

        bool isEmployee = _userContext.Roles.Contains("Employee", StringComparer.OrdinalIgnoreCase)
            && !_userContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)
            && !_userContext.Roles.Contains("Evaluator", StringComparer.OrdinalIgnoreCase);
        _logger.LogInformation("FEEDBACK DEBUG: Role check — isEmployee={IsEmployee}, UserId={UserId}", isEmployee, _userContext.UserId);

        if (isEmployee && _userContext.UserId.HasValue)
        {
            _logger.LogInformation("FEEDBACK DEBUG: Applying employee filter — keeping feedback where EmployeeId == {UserId}", _userContext.UserId.Value);
            Guid filterUserId = _userContext.UserId.Value;
            int beforeFilter = items.Count;
            items = items.Where(f => f.EmployeeId == filterUserId).ToList();
            _logger.LogInformation("FEEDBACK DEBUG: After employee filter: {Before} → {After} records", beforeFilter, items.Count);
            if (items.Count == 0 && beforeFilter > 0)
            {
                _logger.LogWarning("FEEDBACK DEBUG: ALL RECORDS FILTERED OUT — UserId={UserId} does not match EmployeeId on any feedback record", filterUserId);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            items = items
                .Where(f => f.Comment.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || f.FeedbackTypeName.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        int totalRecords = items.Count;
        _logger.LogInformation("FEEDBACK DEBUG: Final records to paginate = {Count}", totalRecords);

        List<FbEntity> paged = items
            .OrderBy(f => f.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<FeedbackDto> data = paged.Select(f => new FeedbackDto
        {
            FeedbackId = f.Id,
            EmployeeId = f.EmployeeId,
            ProgramId = enrollment?.ProgramId ?? 0,
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
