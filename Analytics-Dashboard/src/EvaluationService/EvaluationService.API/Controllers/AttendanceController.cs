using EvaluationService.Application.Dtos.Attendance;
using EvaluationService.Application.Features.Attendance.Commands;
using EvaluationService.Application.Features.Attendance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace EvaluationService.API.Controllers;

/// <summary>
/// API controller for managing attendance records for a given enrollment.
/// </summary>
[ApiController]
[Route("enrollment/{enrollmentId:long}/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IMediator mediator, ILogger<AttendanceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    
    /// <summary>
    /// Retrieves a paginated list of attendance records for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of attendance records.</returns>
    [HttpGet]
    [FeatureAuthorize("EVAL_ATTENDANCE_READ")]
    public async Task<ActionResult<AttendanceListResponseDto>> GetAll(
        Int64 enrollmentId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all attendances for enrollment {EnrollmentId} with page {PageNumber} size {PageSize}", enrollmentId, pageNumber, pageSize);
        AttendanceListResponseDto result = await _mediator.Send(
            new GetAllAttendancesQuery(enrollmentId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    
    /// <summary>
    /// Creates a new attendance record for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="dto">The attendance creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created attendance identifier.</returns>
    [HttpPost]
    [FeatureAuthorize("EVAL_ATTENDANCE_WRITE")]
    public async Task<ActionResult<AttendanceCreatedDto>> Create(
        Int64 enrollmentId,
        [FromBody] CreateAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating attendance for enrollment {EnrollmentId}", enrollmentId);
        AttendanceCreatedDto result = await _mediator.Send(
            new CreateAttendanceCommand(enrollmentId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing attendance record for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="attendanceId">The attendance record identifier.</param>
    /// <param name="dto">The attendance update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated attendance record.</returns>
    [HttpPut("{attendanceId:long}")]
    [FeatureAuthorize("EVAL_ATTENDANCE_WRITE")]
    public async Task<ActionResult<AttendanceDto>> Update(
        Int64 enrollmentId,
        Int64 attendanceId,
        [FromBody] UpdateAttendanceDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating attendance {AttendanceId} for enrollment {EnrollmentId}", attendanceId, enrollmentId);
        AttendanceDto result = await _mediator.Send(
            new UpdateAttendanceCommand(enrollmentId, attendanceId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an attendance record for the specified enrollment.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="attendanceId">The attendance record identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{attendanceId:long}")]
    [FeatureAuthorize("EVAL_ATTENDANCE_WRITE")]
    public async Task<IActionResult> Delete(
        Int64 enrollmentId,
        Int64 attendanceId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting attendance {AttendanceId} for enrollment {EnrollmentId}", attendanceId, enrollmentId);
        await _mediator.Send(
            new DeleteAttendanceCommand(enrollmentId, attendanceId), cancellationToken);
        return NoContent();
    }
}
