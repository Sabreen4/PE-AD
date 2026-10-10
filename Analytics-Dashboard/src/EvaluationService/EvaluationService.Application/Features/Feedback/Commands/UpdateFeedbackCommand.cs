using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Feedback;
using EvaluationService.Application.Features.Leaderboard.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Program;
using Shared.InterServiceHelper.Interfaces;
using FbEntity = EvaluationService.Domain.Entities.Feedback;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;
using MetaRefTermDto = Shared.InterServiceHelper.Grpc.MetaData.RefTermDto;

namespace EvaluationService.Application.Features.Feedback.Commands;

/// <summary>
/// Command to update an existing feedback entry.
/// </summary>
/// <param name="FeedbackId">The feedback identifier.</param>
/// <param name="Dto">The feedback update data.</param>
public record UpdateFeedbackCommand(long FeedbackId, UpdateFeedbackDto Dto) : IRequest<FeedbackDto>;

/// <summary>
/// Handles the update of a feedback entry, including validation and leaderboard recalculation.
/// </summary>
public class UpdateFeedbackCommandHandler : IRequestHandler<UpdateFeedbackCommand, FeedbackDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateFeedbackCommandHandler> _logger;

    private static readonly Dictionary<string, decimal> ScoreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GREEN_FLAG", 3m },
        { "RED_FLAG", -2m }
    };

    public UpdateFeedbackCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        IMapper mapper,
        IMediator mediator,
        ILogger<UpdateFeedbackCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by validating and updating the feedback, then recalculating the leaderboard.
    /// </summary>
    /// <returns>The updated feedback details.</returns>
    public async Task<FeedbackDto> Handle(UpdateFeedbackCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating feedback {FeedbackId}", request.FeedbackId);

        FbEntity? existing = await _evaluationService.GetFeedbackByIdAsync(request.FeedbackId);
        if (existing is null)
        {
            _logger.LogWarning("Feedback not found: {FeedbackId}", request.FeedbackId);
            throw new NotFoundException("Feedback", request.FeedbackId);
        }

        if (!ScoreMap.ContainsKey(request.Dto.FeedbackTypeCode))
        {
            throw new BadRequestException($"'{request.Dto.FeedbackTypeCode}' is not a valid feedback type. Allowed types: GREEN_FLAG, RED_FLAG.");
        }

        IReadOnlyList<MetaRefTermDto> feedbackTypes = await _metaDataClient.GetRefTermsBySetAsync("FeedbackType", cancellationToken);
        MetaRefTermDto? feedbackType = feedbackTypes.FirstOrDefault(rt =>
            rt.Key.Equals(request.Dto.FeedbackTypeCode, StringComparison.OrdinalIgnoreCase));
        if (feedbackType is null)
        {
            _logger.LogWarning("Feedback type '{FeedbackTypeCode}' not found in Metadata Service", request.Dto.FeedbackTypeCode);
            throw new BadRequestException($"'{request.Dto.FeedbackTypeCode}' is not a valid feedback type.");
        }

        WEEntity? weeklyEval = await _evaluationService.GetWeeklyEvaluationByIdAsync(existing.WeeklyEvaluationId);
        if (weeklyEval is null)
        {
            _logger.LogWarning("WeeklyEvaluation not found: {WeeklyEvaluationId}", existing.WeeklyEvaluationId);
            throw new NotFoundException("WeeklyEvaluation", existing.WeeklyEvaluationId);
        }

        GetEmployeeAssignmentByIdResponse? assignment =
            await _programClient.GetEmployeeAssignmentByIdAsync(weeklyEval.EmployeeAssignmentId, cancellationToken);
        if (assignment is null)
        {
            _logger.LogWarning("EmployeeAssignment not found: {EmployeeAssignmentId}", weeklyEval.EmployeeAssignmentId);
            throw new NotFoundException("EmployeeAssignment", weeklyEval.EmployeeAssignmentId);
        }

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(assignment.EnrollmentId, cancellationToken);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", assignment.EnrollmentId);
            throw new NotFoundException("Enrollment", assignment.EnrollmentId);
        }

        GetProgramWeekResponse? programWeek = await _programClient.GetProgramWeekByIdAsync(weeklyEval.ProgramWeekId, cancellationToken: cancellationToken);

        decimal adjustment = ScoreMap[request.Dto.FeedbackTypeCode];

        existing.FeedbackTypeId = Guid.Parse(feedbackType.Id);
        existing.FeedbackTypeCode = feedbackType.Key;
        existing.FeedbackTypeName = string.IsNullOrWhiteSpace(feedbackType.Description)
            ? feedbackType.Key
            : feedbackType.Description;
        existing.FeedbackScoreAdjustment = adjustment;
        existing.Comment = request.Dto.Comment;

        _evaluationService.UpdateFeedback(existing);
        await _evaluationService.SaveAsync();

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        FeedbackDto result = new FeedbackDto
        {
            FeedbackId = existing.Id,
            EmployeeId = existing.EmployeeId,
            ProgramId = enrollment.ProgramId,
            ProgramWeekId = weeklyEval.ProgramWeekId,
            WeeklyEvaluationId = existing.WeeklyEvaluationId,
            FeedbackTypeId = existing.FeedbackTypeId,
            FeedbackTypeCode = existing.FeedbackTypeCode,
            FeedbackTypeName = existing.FeedbackTypeName,
            FeedbackScoreAdjustment = existing.FeedbackScoreAdjustment,
            FeedbackComment = existing.Comment,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt
        };

        return result;
    }
}
