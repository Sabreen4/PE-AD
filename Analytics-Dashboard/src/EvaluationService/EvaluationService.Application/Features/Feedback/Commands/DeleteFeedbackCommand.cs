using EvaluationService.Application.Contracts;
using EvaluationService.Application.Features.Leaderboard.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Program;
using FbEntity = EvaluationService.Domain.Entities.Feedback;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;

namespace EvaluationService.Application.Features.Feedback.Commands;

/// <summary>
/// Command to delete a feedback entry.
/// </summary>
/// <param name="FeedbackId">The feedback identifier.</param>
public record DeleteFeedbackCommand(long FeedbackId) : IRequest<Unit>;

/// <summary>
/// Handles the deletion of a feedback entry, including validation and leaderboard recalculation.
/// </summary>
public class DeleteFeedbackCommandHandler : IRequestHandler<DeleteFeedbackCommand, Unit>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteFeedbackCommandHandler> _logger;

    public DeleteFeedbackCommandHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IMediator mediator,
        ILogger<DeleteFeedbackCommandHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by validating and deleting the feedback, then recalculating the leaderboard.
    /// </summary>
    /// <returns>A unit value indicating completion.</returns>
    public async Task<Unit> Handle(DeleteFeedbackCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting feedback {FeedbackId}", request.FeedbackId);

        FbEntity? existing = await _evaluationService.GetFeedbackByIdAsync(request.FeedbackId);
        if (existing is null)
        {
            _logger.LogWarning("Feedback not found: {FeedbackId}", request.FeedbackId);
            throw new NotFoundException("Feedback", request.FeedbackId);
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

        _evaluationService.DeleteFeedback(existing);
        await _evaluationService.SaveAsync();

        await _mediator.Send(new RecalculateLeaderboardCommand(enrollment.ProgramId), cancellationToken);

        return Unit.Value;
    }
}
