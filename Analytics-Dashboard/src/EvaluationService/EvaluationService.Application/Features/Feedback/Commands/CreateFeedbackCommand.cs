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
/// Command to create or update feedback for a weekly evaluation.
/// </summary>
/// <param name="Dto">The feedback creation data.</param>
public record CreateFeedbackCommand(CreateFeedbackDto Dto) : IRequest<FeedbackCreatedDto>;

/// <summary>
/// Handles the creation or update of feedback, including validation, metadata lookup, leaderboard recalculation, and notification sending.
/// </summary>
public class CreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, FeedbackCreatedDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateFeedbackCommandHandler> _logger;

    private static readonly Dictionary<string, decimal> ScoreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GREEN_FLAG", 3m },
        { "RED_FLAG", -2m }
    };

    public CreateFeedbackCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        IMapper mapper,
        IMediator mediator,
        ILogger<CreateFeedbackCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by validating the feedback type, enrollment, and program week, then creating or updating the feedback and sending notifications.
    /// </summary>
    /// <returns>The created or updated feedback details.</returns>
    public async Task<FeedbackCreatedDto> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating feedback for EmployeeId: {EmployeeId}, WeeklyEvaluationId: {WeeklyEvaluationId}",
            request.Dto.EmployeeId, request.Dto.WeeklyEvaluationId);

        ValidateFeedbackTypeCode(request.Dto.FeedbackTypeCode);

        WEEntity? weeklyEval = await _evaluationService.GetWeeklyEvaluationByIdAsync(request.Dto.WeeklyEvaluationId);
        if (weeklyEval is null)
        {
            _logger.LogWarning("WeeklyEvaluation not found: {WeeklyEvaluationId}", request.Dto.WeeklyEvaluationId);
            throw new NotFoundException("WeeklyEvaluation", request.Dto.WeeklyEvaluationId);
        }

        GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(weeklyEval.EnrollmentId, cancellationToken);
        if (enrollment is null || !enrollment.Found)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", weeklyEval.EnrollmentId);
            throw new NotFoundException("Enrollment", weeklyEval.EnrollmentId);
        }

        if (!enrollment.IsActive)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} is not active", weeklyEval.EnrollmentId);
            throw new BadRequestException("The enrollment is not active.");
        }

        if (Guid.Parse(enrollment.EmployeeId) != request.Dto.EmployeeId)
        {
            _logger.LogWarning("Employee {EmployeeId} does not match enrollment's employee {EnrollmentEmployeeId}",
                request.Dto.EmployeeId, enrollment.EmployeeId);
            throw new BadRequestException("The employee does not match the weekly evaluation's enrollment.");
        }

        GetProgramWeekResponse? programWeek = await _programClient.GetProgramWeekByIdAsync(weeklyEval.ProgramWeekId, cancellationToken: cancellationToken);
        if (programWeek is null || !programWeek.Found)
        {
            _logger.LogWarning("ProgramWeek not found: {ProgramWeekId}", weeklyEval.ProgramWeekId);
            throw new NotFoundException("ProgramWeek", weeklyEval.ProgramWeekId);
        }

        if (programWeek.ProgramId != enrollment.ProgramId)
        {
            _logger.LogWarning("ProgramWeek {ProgramWeekId} does not belong to Program {ProgramId}", weeklyEval.ProgramWeekId, enrollment.ProgramId);
            throw new BadRequestException("ProgramWeek does not belong to the enrollment's program.");
        }

        IReadOnlyList<MetaRefTermDto> feedbackTypes = await _metaDataClient.GetRefTermsBySetAsync("FeedbackType", cancellationToken);
        MetaRefTermDto? feedbackType = feedbackTypes.FirstOrDefault(rt =>
            rt.Key.Equals(request.Dto.FeedbackTypeCode, StringComparison.OrdinalIgnoreCase));
        if (feedbackType is null)
        {
            _logger.LogWarning("Feedback type '{FeedbackTypeCode}' not found in Metadata Service", request.Dto.FeedbackTypeCode);
            throw new BadRequestException($"'{request.Dto.FeedbackTypeCode}' is not a valid feedback type.");
        }

        List<FbEntity> existingFeedbacks = await _evaluationService.GetFeedbacksByEmployeeAsync(request.Dto.EmployeeId);
        FbEntity? existing = existingFeedbacks.FirstOrDefault(f =>
        {
            WEEntity? eval = _evaluationService.GetWeeklyEvaluationByIdAsync(f.WeeklyEvaluationId).GetAwaiter().GetResult();
            return eval?.ProgramWeekId == weeklyEval.ProgramWeekId;
        });

        decimal adjustment = ScoreMap[request.Dto.FeedbackTypeCode];

        if (existing is not null)
        {
            existing.WeeklyEvaluationId = request.Dto.WeeklyEvaluationId;
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

            await _notificationClient.SendInAppNotificationAsync(
                request.Dto.EmployeeId,
                "Feedback Updated",
                $"Your feedback has been updated to {request.Dto.FeedbackTypeCode}.");

            FeedbackDto feedbackDto = BuildFeedbackDto(existing, weeklyEval, enrollment, programWeek);
            return new FeedbackCreatedDto { Id = existing.Id, Feedback = feedbackDto };
        }

        FbEntity entity = new FbEntity
        {
            WeeklyEvaluationId = request.Dto.WeeklyEvaluationId,
            EmployeeId = request.Dto.EmployeeId,
            FeedbackTypeId = Guid.Parse(feedbackType.Id),
            FeedbackTypeCode = feedbackType.Key,
            FeedbackTypeName = string.IsNullOrWhiteSpace(feedbackType.Description)
                ? feedbackType.Key
                : feedbackType.Description,
            FeedbackScoreAdjustment = adjustment,
            Comment = request.Dto.Comment
        };

        await _evaluationService.CreateFeedbackAsync(entity);
        await _evaluationService.SaveAsync();

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        await _notificationClient.SendInAppNotificationAsync(
            request.Dto.EmployeeId,
            "Feedback Received",
            $"You have received {request.Dto.FeedbackTypeCode} feedback.");

        FeedbackDto resultDto = BuildFeedbackDto(entity, weeklyEval, enrollment, programWeek);
        return new FeedbackCreatedDto { Id = entity.Id, Feedback = resultDto };
    }

    /// <summary>
    /// Validates that the feedback type code is not empty and is a recognized type (GREEN_FLAG or RED_FLAG).
    /// </summary>
    /// <param name="code">The feedback type code to validate.</param>
    /// <exception cref="BadRequestException">Thrown when the code is invalid.</exception>
    private static void ValidateFeedbackTypeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BadRequestException("FeedbackTypeCode is required.");

        if (!ScoreMap.ContainsKey(code))
            throw new BadRequestException($"'{code}' is not a valid feedback type. Allowed types: GREEN_FLAG, RED_FLAG.");
    }

    /// <summary>
    /// Builds a <see cref="FeedbackDto"/> from the given entity and related data.
    /// </summary>
    /// <param name="entity">The feedback entity.</param>
    /// <param name="weeklyEval">The associated weekly evaluation.</param>
    /// <param name="enrollment">The enrollment response.</param>
    /// <param name="programWeek">The program week response.</param>
    /// <returns>A populated <see cref="FeedbackDto"/>.</returns>
    private static FeedbackDto BuildFeedbackDto(FbEntity entity, WEEntity weeklyEval, GetEnrollmentResponse enrollment, GetProgramWeekResponse programWeek)
    {
        return new FeedbackDto
        {
            FeedbackId = entity.Id,
            EmployeeId = entity.EmployeeId,
            ProgramId = enrollment.ProgramId,
            ProgramWeekId = weeklyEval.ProgramWeekId,
            WeeklyEvaluationId = entity.WeeklyEvaluationId,
            FeedbackTypeId = entity.FeedbackTypeId,
            FeedbackTypeCode = entity.FeedbackTypeCode,
            FeedbackTypeName = entity.FeedbackTypeName,
            FeedbackScoreAdjustment = entity.FeedbackScoreAdjustment,
            FeedbackComment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
