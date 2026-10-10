using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgramService.Application.Dtos.Assignments;
using ProgramService.Application.Features.Assignments.Commands;
using ProgramService.Application.Features.Assignments.Queries;
using Shared.Authorization.Attributes;
using Shared.Common.Dtos;

namespace ProgramService.API.Controllers;

[ApiController]

[Route("program/{programId:long}/program-week/{weekId:long}/assignment")]
/// <summary>
/// Controller for managing assignments within program weeks.
/// </summary>
public class AssignmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AssignmentController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AssignmentController"/>.
    /// </summary>
    public AssignmentController(IMediator mediator, ILogger<AssignmentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of all assignments for the specified program week.
    /// </summary>
    /// <returns>A paginated response containing assignment data.</returns>
    [HttpGet]
   [FeatureAuthorize("ASSIGNMENT_VIEW")]
    public async Task<ActionResult<PagedResponseDto<AssignmentDto>>> GetAll(
        Int64 programId,
        Int64 weekId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting assignments for program {ProgramId} week {WeekId} with page {PageNumber} size {PageSize} search '{Search}'", programId, weekId, pageNumber, pageSize, search);
        PagedResponseDto<AssignmentDto> result = await _mediator.Send(
            new GetAllAssignmentsQuery(programId, weekId, pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific assignment by its ID within the given program and week.
    /// </summary>
    /// <returns>The assignment detail data.</returns>
    [HttpGet("{assignmentId:long}")]
    [FeatureAuthorize("ASSIGNMENT_VIEW")]
    public async Task<ActionResult<AssignmentDetailDto>> GetById(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting assignment {AssignmentId} for program {ProgramId} week {WeekId}", assignmentId, programId, weekId);
        AssignmentDetailDto result = await _mediator.Send(
            new GetAssignmentQuery(programId, weekId, assignmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new assignment for the specified program week.
    /// </summary>
    /// <returns>The created assignment data.</returns>
    [HttpPost]
    [FeatureAuthorize("ASSIGNMENT_CREATE")]
    public async Task<ActionResult<AssignmentCreatedDto>> Create(
        Int64 programId,
        Int64 weekId,
        [FromBody] CreateAssignmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating assignment for program {ProgramId} week {WeekId}", programId, weekId);
        AssignmentCreatedDto result = await _mediator.Send(
            new CreateAssignmentCommand(programId, weekId, dto), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Updates an existing assignment within the given program and week.
    /// </summary>
    /// <returns>The updated assignment detail data.</returns>
    [HttpPut("{assignmentId:long}")]
    [FeatureAuthorize("ASSIGNMENT_UPDATE")]
    public async Task<ActionResult<AssignmentDetailDto>> Update(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        [FromBody] UpdateAssignmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating assignment {AssignmentId} for program {ProgramId} week {WeekId}", assignmentId, programId, weekId);
        AssignmentDetailDto result = await _mediator.Send(
            new UpdateAssignmentCommand(programId, weekId, assignmentId, dto), cancellationToken);
        return Ok(result);
    }
}

