using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Application.Features.Programs.Commands;
using ProgramService.Application.Features.Programs.Queries;
using Shared.Authorization.Attributes;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;

namespace ProgramService.API.Controllers;

[ApiController]
[Route("program")]
/// <summary>
/// Controller for managing training programs.
/// </summary>
public class ProgramController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContext;
    private readonly ILogger<ProgramController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProgramController"/>.
    /// </summary>
    public ProgramController(IMediator mediator, IUserContextService userContext, ILogger<ProgramController> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of all training programs.
    /// </summary>
    /// <returns>A paginated response containing program data.</returns>
    [HttpGet]
    [FeatureAuthorize("PROGRAM_VIEW")]
    public async Task<ActionResult<PagedResponseDto<ProgramDto>>> GetAll(
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all programs with page {PageNumber} size {PageSize} search '{Search}'", pageNumber, pageSize, search);
        PagedResponseDto<ProgramDto> result = await _mediator.Send(
            new GetAllProgramsQuery(pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific training program by its ID.
    /// </summary>
    /// <returns>The program detail data.</returns>
    [HttpGet("{programId:long}")]
    [FeatureAuthorize("PROGRAM_VIEW")]
    public async Task<ActionResult<ProgramDetailDto>> GetById(
        Int64 programId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting program {ProgramId}", programId);
        ProgramDetailDto result = await _mediator.Send(
            new GetProgramQuery(programId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new training program.
    /// </summary>
    /// <returns>The created program data.</returns>
    [HttpPost]
    [FeatureAuthorize("PROGRAM_CREATE")]
    public async Task<ActionResult<ProgramCreatedDto>> Create(
        [FromBody] CreateProgramDto dto,
        CancellationToken cancellationToken)
    {
        Guid evaluatorId = _userContext.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
        Int64 organizationId = _userContext.OrganizationId;
        _logger.LogInformation("Creating program with name {ProgramName} for organization {OrganizationId}", dto.Name, organizationId);
        ProgramCreatedDto result = await _mediator.Send(
            new CreateProgramCommand(dto, evaluatorId, organizationId), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Updates an existing training program.
    /// </summary>
    /// <returns>The updated program detail data.</returns>
    [HttpPut("{programId:long}")]
    [FeatureAuthorize("PROGRAM_UPDATE")]
    public async Task<ActionResult<ProgramDetailDto>> Update(
        Int64 programId,
        [FromBody] UpdateProgramDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating program {ProgramId}", programId);
        ProgramDetailDto result = await _mediator.Send(
            new UpdateProgramCommand(programId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Publishes a training program, notifying all enrolled employees.
    /// </summary>
    /// <returns>The published program detail data.</returns>
    [HttpPost("{programId:long}/publish")]
    [FeatureAuthorize("PROGRAM_PUBLISH")]
    public async Task<ActionResult<ProgramDetailDto>> Publish(
        Int64 programId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing program {ProgramId}", programId);
        ProgramDetailDto result = await _mediator.Send(
            new PublishProgramCommand(programId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a training program by its ID.
    /// </summary>
    /// <returns>No content.</returns>
    [HttpDelete("{programId:long}")]
    [FeatureAuthorize("PROGRAM_DELETE")]
    public async Task<IActionResult> Delete(
        Int64 programId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting program {ProgramId}", programId);
        await _mediator.Send(
            new DeleteProgramCommand(programId), cancellationToken);
        return NoContent();
    }
}
