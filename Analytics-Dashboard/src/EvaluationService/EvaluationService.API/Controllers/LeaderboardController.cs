using EvaluationService.Application.Dtos.Leaderboard;
using EvaluationService.Application.Features.Leaderboard.Commands;
using EvaluationService.Application.Features.Leaderboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace EvaluationService.API.Controllers;

/// <summary>
/// API controller for retrieving and recalculating the leaderboard for a program.
/// </summary>
[ApiController]
[Route("program/{programId:long}/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(IMediator mediator, ILogger<LeaderboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated leaderboard for the specified program.
    /// </summary>
    /// <param name="programId">The program identifier.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of leaderboard entries.</returns>
    [HttpGet]
    [FeatureAuthorize("EVAL_LEADERBOARD_READ")]
    public async Task<ActionResult<LeaderboardListResponseDto>> GetAll(
        Int64 programId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting leaderboard for program {ProgramId} with page {PageNumber} size {PageSize}", programId, pageNumber, pageSize);
        LeaderboardListResponseDto result = await _mediator.Send(
            new GetLeaderboardQuery(programId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Recalculates the leaderboard scores for all enrollments in the specified program.
    /// </summary>
    /// <param name="programId">The program identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated leaderboard.</returns>
    [HttpPost("recalculate")]
    [FeatureAuthorize("EVAL_LEADERBOARD_WRITE")]
    public async Task<ActionResult<LeaderboardListResponseDto>> Recalculate(
        Int64 programId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recalculating leaderboard for program {ProgramId}", programId);
        LeaderboardListResponseDto result = await _mediator.Send(
            new RecalculateLeaderboardCommand(programId), cancellationToken);
        return Ok(result);
    }
}
