using EvaluationService.Application.Dtos.WeeklyEvaluation;
using EvaluationService.Application.Features.WeeklyEvaluation.Commands;
using EvaluationService.Application.Features.WeeklyEvaluation.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace EvaluationService.API.Controllers;

/// <summary>
/// API controller for managing weekly evaluations for a given enrollment.
/// </summary>
[ApiController]
[Route("enrollment/{enrollmentId:long}/weekly-evaluation")]
public class WeeklyEvaluationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WeeklyEvaluationController> _logger;

    public WeeklyEvaluationController(IMediator mediator, ILogger<WeeklyEvaluationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of weekly evaluations for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of weekly evaluations.</returns>
    [HttpGet]
    [FeatureAuthorize("EVAL_WEEKLY_EVAL_READ")]
    public async Task<ActionResult<WeeklyEvaluationListResponseDto>> GetAll(
        Int64 enrollmentId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== WEEKLY EVAL DEBUG: Getting weekly evaluations for enrollment {EnrollmentId}, page {PageNumber}, size {PageSize}", enrollmentId, pageNumber, pageSize);
        WeeklyEvaluationListResponseDto result = await _mediator.Send(
            new GetAllWeeklyEvaluationsQuery(enrollmentId, pageNumber, pageSize), cancellationToken);
        var evalIds = result?.Data != null
            ? string.Join(", ", result.Data.Select(e => $"Id={e.Id},ProgramWeekId={e.ProgramWeekId}"))
            : "null";
        _logger.LogInformation("WEEKLY EVAL DEBUG: Returned {Count} evaluations: [{EvalIds}]", result?.Data?.Count ?? 0, evalIds);
        return Ok(result);
    }

    /// <summary>
    /// Calculates and returns the attendance score (out of 3) based on daily attendance records for the given enrollment and program week.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The attendance score (0–3).</returns>
    [HttpGet("{programWeekId:long}/attendance-score")]
    [FeatureAuthorize("EVAL_WEEKLY_EVAL_READ")]
    public async Task<ActionResult<int>> GetAttendanceScore(
        Int64 enrollmentId,
        Int64 programWeekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating attendance score for enrollment {EnrollmentId}, week {ProgramWeekId}", enrollmentId, programWeekId);
        int score = await _mediator.Send(
            new GetAttendanceScoreQuery(enrollmentId, programWeekId), cancellationToken);
        return Ok(score);
    }

    /// <summary>
    /// Creates a new weekly evaluation for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="dto">The evaluation creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created weekly evaluation identifier.</returns>
    [HttpPost]
    [FeatureAuthorize("EVAL_WEEKLY_EVAL_WRITE")]
    public async Task<ActionResult<WeeklyEvaluationCreatedDto>> Create(
        Int64 enrollmentId,
        [FromBody] CreateWeeklyEvaluationDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating weekly evaluation for enrollment {EnrollmentId}", enrollmentId);
        WeeklyEvaluationCreatedDto result = await _mediator.Send(
            new CreateWeeklyEvaluationCommand(enrollmentId, dto), cancellationToken);
        return Ok(result);
    }
}
