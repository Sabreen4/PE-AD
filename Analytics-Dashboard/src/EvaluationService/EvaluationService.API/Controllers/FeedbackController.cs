using EvaluationService.Application.Dtos.Feedback;
using EvaluationService.Application.Features.Feedback.Commands;
using EvaluationService.Application.Features.Feedback.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace EvaluationService.API.Controllers;

/// <summary>
/// API controller for managing feedback entries on weekly evaluations.
/// </summary>
[ApiController]
[Route("feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IMediator mediator, ILogger<FeedbackController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of feedback entries for the specified weekly evaluation.
    /// </summary>
    /// <param name="weeklyEvaluationId">The weekly evaluation identifier.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of feedback entries.</returns>
    [HttpGet("by-weekly-evaluation/{weeklyEvaluationId:long}")]
    [FeatureAuthorize("EVAL_FEEDBACK_READ")]
    public async Task<ActionResult<FeedbackListResponseDto>> GetByWeeklyEvaluation(
        long weeklyEvaluationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting feedback for weekly evaluation {WeeklyEvaluationId} search '{Search}'", weeklyEvaluationId, search);
        FeedbackListResponseDto result = await _mediator.Send(
            new GetAllFeedbacksQuery(weeklyEvaluationId, pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of feedback entries for a specific enrollment and program week.
    /// Resolves the weekly evaluation internally by enrollment and program week.
    /// </summary>
    [HttpGet("by-enrollment/{enrollmentId:long}/program-week/{programWeekId:long}")]
    [FeatureAuthorize("EVAL_FEEDBACK_READ")]
    public async Task<ActionResult<FeedbackListResponseDto>> GetByEnrollmentAndWeek(
        long enrollmentId,
        long programWeekId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting feedback for enrollment {EnrollmentId}, program week {ProgramWeekId} search '{Search}'", enrollmentId, programWeekId, search);
        FeedbackListResponseDto result = await _mediator.Send(
            new GetFeedbackByEnrollmentWeekQuery(enrollmentId, programWeekId, pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new feedback entry for a weekly evaluation.
    /// </summary>
    /// <param name="dto">The feedback creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created feedback details.</returns>
    [HttpPost]
    [FeatureAuthorize("EVAL_FEEDBACK_WRITE")]
    public async Task<ActionResult<FeedbackCreatedDto>> Create(
        [FromBody] CreateFeedbackDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating feedback for EmployeeId: {EmployeeId}, WeeklyEvaluationId: {WeeklyEvaluationId}", dto.EmployeeId, dto.WeeklyEvaluationId);
        FeedbackCreatedDto result = await _mediator.Send(
            new CreateFeedbackCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing feedback entry.
    /// </summary>
    /// <param name="feedbackId">The feedback identifier.</param>
    /// <param name="dto">The feedback update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated feedback details.</returns>
    [HttpPut("{feedbackId:long}")]
    [FeatureAuthorize("EVAL_FEEDBACK_WRITE")]
    public async Task<ActionResult<FeedbackDto>> Update(
        long feedbackId,
        [FromBody] UpdateFeedbackDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating feedback {FeedbackId}", feedbackId);
        FeedbackDto result = await _mediator.Send(
            new UpdateFeedbackCommand(feedbackId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a feedback entry.
    /// </summary>
    /// <param name="feedbackId">The feedback identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{feedbackId:long}")]
    [FeatureAuthorize("EVAL_FEEDBACK_WRITE")]
    public async Task<ActionResult> Delete(
        long feedbackId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting feedback {FeedbackId}", feedbackId);
        await _mediator.Send(new DeleteFeedbackCommand(feedbackId), cancellationToken);
        return NoContent();
    }
}
