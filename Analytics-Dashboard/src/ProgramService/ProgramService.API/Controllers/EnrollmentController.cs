using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Application.Features.Enrollments.Commands;
using ProgramService.Application.Features.Enrollments.Queries;
using Shared.Authorization.Attributes;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;

namespace ProgramService.API.Controllers;

[ApiController]
[Route("program/{programId:long}/enrollment")]
/// <summary>
/// Controller for managing program enrollments.
/// </summary>
public class EnrollmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContext;
    private readonly ILogger<EnrollmentController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="EnrollmentController"/>.
    /// </summary>
    public EnrollmentController(IMediator mediator, IUserContextService userContext, ILogger<EnrollmentController> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of enrollments for the specified program.
    /// </summary>
    /// <returns>A paginated response containing enrollment data.</returns>
    [HttpGet]
    [FeatureAuthorize("ENROLLMENT_VIEW")]
    public async Task<ActionResult<PagedResponseDto<EnrollmentDto>>> GetAll(
        Int64 programId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting enrollments for program {ProgramId} with page {PageNumber} size {PageSize}", programId, pageNumber, pageSize);
        PagedResponseDto<EnrollmentDto> result = await _mediator.Send(
            new GetAllEnrollmentsQuery(programId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific enrollment by its ID.
    /// </summary>
    /// <returns>The enrollment detail data.</returns>
    [HttpGet("{enrollmentId:long}")]
    [FeatureAuthorize("ENROLLMENT_VIEW")]
    public async Task<ActionResult<EnrollmentDetailDto>> GetById(
        Int64 programId,
        Int64 enrollmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting enrollment {EnrollmentId} for program {ProgramId}", enrollmentId, programId);
        EnrollmentDetailDto result = await _mediator.Send(
            new GetEnrollmentQuery(programId, enrollmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the enrollment of the currently authenticated employee for the specified program.
    /// </summary>
    /// <returns>The enrollment detail data for the current employee.</returns>
    [HttpGet("my-enrollment")]
    [FeatureAuthorize("EMPLOYEE_ENROLLMENT_VIEW")]
    public async Task<ActionResult<EnrollmentDetailDto>> GetMyEnrollment(
        Int64 programId,
        CancellationToken cancellationToken)
    {
        Guid userId = _userContext.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
        _logger.LogInformation("Getting my enrollment for program {ProgramId} user {UserId}", programId, userId);
        EnrollmentDetailDto result = await _mediator.Send(
            new GetMyEnrollmentQuery(programId, userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new enrollment for an employee in the specified program.
    /// </summary>
    /// <returns>The created enrollment data.</returns>
    [HttpPost]
    [FeatureAuthorize("ENROLLMENT_CREATE")]
    public async Task<ActionResult<EnrollmentCreatedDto>> Create(
        Int64 programId,
        [FromQuery] Guid employeeId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating enrollment for program {ProgramId} employee {EmployeeId}", programId, employeeId);
        EnrollmentCreatedDto result = await _mediator.Send(
            new CreateEnrollmentCommand(programId, employeeId), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Deletes an enrollment by its ID.
    /// </summary>
    /// <returns>No content.</returns>
    [HttpDelete("{enrollmentId:long}")]
    [FeatureAuthorize("ENROLLMENT_DELETE")]
    public async Task<IActionResult> Delete(
        Int64 programId,
        Int64 enrollmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting enrollment {EnrollmentId} for program {ProgramId}", enrollmentId, programId);
        await _mediator.Send(
            new DeleteEnrollmentCommand(programId, enrollmentId), cancellationToken);
        return NoContent();
    }
}

