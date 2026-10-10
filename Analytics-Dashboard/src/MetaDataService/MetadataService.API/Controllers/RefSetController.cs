using MediatR;
using Microsoft.AspNetCore.Mvc;
using MetaDataService.Application.Dtos;
using MetaDataService.Application.Features.RefSets.Queries;
using Shared.Authorization.Attributes;
using Shared.Common.Dtos;

namespace MetaDataService.API.Controllers;

[ApiController]
[Route("ref-sets")]
[FeatureAuthorize("FEEDBACK_PROVIDE")]
/// <summary>
/// REST controller for managing RefSet-related operations.
/// </summary>
public sealed class RefSetController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RefSetController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefSetController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR instance.</param>
    /// <param name="logger">The logger instance.</param>
    public RefSetController(IMediator mediator, ILogger<RefSetController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated RefSet terms for the specified key.
    /// </summary>
    /// <param name="key">The RefSet key (name).</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="isActive">Optional filter for active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response of RefSet term DTOs.</returns>
    [HttpGet("{key}/terms")]
    public async Task<ActionResult<PagedResponseDto<RefSetTermResponseDto>>> GetRefSetTerms(
        string key,
        [FromQuery] int page = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        [FromQuery(Name = "is-active")] bool? isActive = null,
        [FromQuery(Name = "search")] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting ref set terms for key {RefSetKey} with page {Page} size {PageSize} search '{Search}'", key, page, pageSize, search);
        PagedResponseDto<RefSetTermResponseDto> result = await _mediator.Send(
            new GetRefSetTermsQuery(key, page, pageSize, isActive, search), cancellationToken);

        return Ok(result);
    }
}
