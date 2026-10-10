using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Application.Features.EmployeeAssignments.Commands;
using ProgramService.Application.Features.EmployeeAssignments.Queries;
using Shared.Authorization.Attributes;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;

namespace ProgramService.API.Controllers;

[ApiController]
[Route("program/{programId:long}/program-week/{weekId:long}/assignment/{assignmentId:long}/employee-assignment")]
/// <summary>
/// Controller for managing employee-specific assignments within a program.
/// </summary>
public class EmployeeAssignmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContext;
    private readonly ILogger<EmployeeAssignmentController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="EmployeeAssignmentController"/>.
    /// </summary>
    public EmployeeAssignmentController(IMediator mediator, IUserContextService userContext, ILogger<EmployeeAssignmentController> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of employee assignments for the given assignment.
    /// </summary>
    /// <returns>A paginated response containing employee assignment data.</returns>
    [HttpGet]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_VIEW")]
    public async Task<ActionResult<PagedResponseDto<EmployeeAssignmentEnrichedDto>>> GetAll(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        [FromQuery] string? search = null,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting employee assignments for assignment {AssignmentId}, search '{Search}', page {PageNumber}, size {PageSize}", assignmentId, search, pageNumber, pageSize);
        PagedResponseDto<EmployeeAssignmentEnrichedDto> result = await _mediator.Send(
            new GetAllEmployeeAssignmentsQuery(programId, weekId, assignmentId, search, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific employee assignment by its ID.
    /// </summary>
    /// <returns>The employee assignment detail data.</returns>
    [HttpGet("{employeeAssignmentId:long}")]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_VIEW")]
    public async Task<ActionResult<EmployeeAssignmentEnrichedDto>> GetById(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        Int64 employeeAssignmentId,
        CancellationToken cancellationToken)
    {
        EmployeeAssignmentEnrichedDto result = await _mediator.Send(
            new GetEmployeeAssignmentQuery(programId, weekId, assignmentId, employeeAssignmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the logged-in employee's own assignment submission for the specified assignment.
    /// </summary>
    /// <returns>The employee assignment detail data for the current employee.</returns>
    [HttpGet("my")]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_VIEW")]
    public async Task<ActionResult<EmployeeAssignmentEnrichedDto>> GetMyAssignment(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
        _logger.LogInformation("Getting my employee assignment for program {ProgramId}, week {WeekId}, assignment {AssignmentId}, user {UserId}",
            programId, weekId, assignmentId, userId);
        EmployeeAssignmentEnrichedDto result = await _mediator.Send(
            new GetMyEmployeeAssignmentQuery(programId, weekId, assignmentId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new employee assignment for the specified employee and assignment.
    /// </summary>
    /// <returns>The created employee assignment response.</returns>
    [HttpPost]
    [FeatureAuthorize("ASSIGNMENT_MANAGE")]
    public async Task<ActionResult<CreateEmployeeAssignmentResponseDto>> Create(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        [FromQuery] Guid employeeId,
        CancellationToken cancellationToken)
    {
        CreateEmployeeAssignmentResponseDto result = await _mediator.Send(
            new CreateEmployeeAssignmentCommand(programId, weekId, assignmentId, employeeId), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Creates a new employee assignment for the currently authenticated user.
    /// </summary>
    /// <returns>The created employee assignment response.</returns>
    [HttpPost("my")]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_SUBMIT")]
    public async Task<ActionResult<CreateEmployeeAssignmentResponseDto>> CreateMyAssignment(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
        _logger.LogInformation("Creating my employee assignment for program {ProgramId}, week {WeekId}, assignment {AssignmentId}, user {UserId}",
            programId, weekId, assignmentId, userId);
        CreateEmployeeAssignmentResponseDto result = await _mediator.Send(
            new CreateEmployeeAssignmentCommand(programId, weekId, assignmentId, userId), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Submits an employee assignment with a file attachment.
    /// </summary>
    /// <returns>The submitted employee assignment response.</returns>
    [HttpPost("{employeeAssignmentId:long}/submit")]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_SUBMIT")]
    public async Task<ActionResult<SubmitEmployeeAssignmentResponseDto>> Submit(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        Int64 employeeAssignmentId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        SubmitEmployeeAssignmentResponseDto result = await _mediator.Send(
            new SubmitEmployeeAssignmentCommand(programId, weekId, assignmentId, employeeAssignmentId, file.OpenReadStream(), file.FileName, file.Length), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Evaluates a submitted employee assignment with marks and reviewer comments.
    /// </summary>
    /// <returns>The evaluated employee assignment detail data.</returns>
    [HttpPut("{employeeAssignmentId:long}/evaluate")]
    [FeatureAuthorize("ASSIGNMENT_EVALUATE")]
    public async Task<ActionResult<EmployeeAssignmentEnrichedDto>> Evaluate(
        Int64 programId,
        Int64 weekId,
        Int64 assignmentId,
        Int64 employeeAssignmentId,
        [FromBody] EvaluateEmployeeAssignmentDto dto,
        CancellationToken cancellationToken)
    {
        EmployeeAssignmentEnrichedDto result = await _mediator.Send(
            new EvaluateEmployeeAssignmentCommand(programId, weekId, assignmentId, employeeAssignmentId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Downloads the submitted file for an employee assignment.
    /// </summary>
    /// <returns>The file as a download stream with the original filename.</returns>
    [HttpGet("{employeeAssignmentId:long}/download")]
    [FeatureAuthorize("EMPLOYEE_ASSIGNMENT_VIEW")]
    public async Task<ActionResult> Download(
        Int64 employeeAssignmentId,
        CancellationToken cancellationToken)
    {
        AssignmentFileDownloadResult result = await _mediator.Send(
            new GetAssignmentFileDownloadQuery(employeeAssignmentId), cancellationToken);
        return PhysicalFile(result.FilePath, result.ContentType, result.FileName);
    }
}

