using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Application.Dtos.Videos;
using ProgramService.Application.Features.ProgramWeeks.Commands;
using ProgramService.Application.Features.ProgramWeeks.Queries;
using Shared.Authorization.Attributes;
using Shared.Common.Dtos;

namespace ProgramService.API.Controllers;

[ApiController]
[Route("program/{programId:long}/program-week")]
/// <summary>
/// Controller for managing program weeks and their video content.
/// </summary>
public class ProgramWeekController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProgramWeekController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProgramWeekController"/>.
    /// </summary>
    public ProgramWeekController(IMediator mediator, ILogger<ProgramWeekController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of program weeks for the specified program.
    /// </summary>
    /// <returns>A paginated response containing program week data.</returns>
    [HttpGet]
    [FeatureAuthorize("PROGRAM_WEEK_VIEW")]
    public async Task<ActionResult<PagedResponseDto<ProgramWeekDto>>> GetAll(
        Int64 programId,
        [FromQuery] Int32 pageNumber = 1,
        [FromQuery] Int32 pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting program weeks for program {ProgramId} with page {PageNumber} size {PageSize} search '{Search}'", programId, pageNumber, pageSize, search);
        PagedResponseDto<ProgramWeekDto> result = await _mediator.Send(
            new GetAllProgramWeeksQuery(programId, pageNumber, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific program week by its ID.
    /// </summary>
    /// <returns>The program week detail data.</returns>
    [HttpGet("{weekId:long}")]
    [FeatureAuthorize("PROGRAM_WEEK_VIEW")]
    public async Task<ActionResult<ProgramWeekDetailDto>> GetById(
        Int64 programId,
        Int64 weekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting program week {WeekId} for program {ProgramId}", weekId, programId);
        ProgramWeekDetailDto result = await _mediator.Send(
            new GetProgramWeekQuery(programId, weekId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new program week for the specified program.
    /// </summary>
    /// <returns>The created program week data.</returns>
    [HttpPost]
   [FeatureAuthorize("PROGRAM_WEEK_CREATE")]
    public async Task<ActionResult<ProgramWeekCreatedDto>> Create(
        Int64 programId,
        [FromBody] CreateProgramWeekDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating program week for program {ProgramId}", programId);
        ProgramWeekCreatedDto result = await _mediator.Send(
            new CreateProgramWeekCommand(programId, dto), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Updates an existing program week.
    /// </summary>
    /// <returns>The updated program week detail data.</returns>
    [HttpPut("{weekId:long}")]
   [FeatureAuthorize("PROGRAM_WEEK_UPDATE")]
    public async Task<ActionResult<ProgramWeekDetailDto>> Update(
        Int64 programId,
        Int64 weekId,
        [FromBody] UpdateProgramWeekDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating program week {WeekId} for program {ProgramId}", weekId, programId);
        ProgramWeekDetailDto result = await _mediator.Send(
            new UpdateProgramWeekCommand(programId, weekId, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Uploads a video file for the specified program week.
    /// </summary>
    /// <returns>The uploaded video response data.</returns>
    [HttpPost("{weekId:long}/video")]
    [FeatureAuthorize("PROGRAM_WEEK_UPDATE")]
    [RequestSizeLimit(15L * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 15L * 1024 * 1024)]
    public async Task<ActionResult<VideoResponseDto>> UploadVideo(
        Int64 programId,
        Int64 weekId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading video for program {ProgramId} week {WeekId} with file {FileName}", programId, weekId, file?.FileName);

        await using Stream? stream = file?.OpenReadStream();
        VideoResponseDto result = await _mediator.Send(new UploadVideoCommand(programId, weekId, stream, file?.FileName ?? string.Empty, file?.Length ?? 0), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Streams the video file for the specified program week.
    /// </summary>
    /// <returns>The video file stream.</returns>
    [HttpGet("{weekId:long}/video")]
    public async Task<ActionResult> StreamVideo(
        Int64 programId,
        Int64 weekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Streaming video for program {ProgramId} week {WeekId}", programId, weekId);
        VideoStreamResult result = await _mediator.Send(
            new GetVideoStreamQuery(programId, weekId), cancellationToken);
        return PhysicalFile(result.FilePath, result.ContentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// Retrieves metadata for the video associated with the specified program week.
    /// </summary>
    /// <returns>The video metadata.</returns>
    [HttpGet("{weekId:long}/video/details")]
    [FeatureAuthorize("PROGRAM_WEEK_VIEW")]
    public async Task<ActionResult<VideoMetadataDto>> GetVideoMetadata(
        Int64 programId,
        Int64 weekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting video metadata for program {ProgramId} week {WeekId}", programId, weekId);
        VideoMetadataDto result = await _mediator.Send(
            new GetVideoMetadataQuery(programId, weekId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes the video associated with the specified program week.
    /// </summary>
    /// <returns>No content.</returns>
    [HttpDelete("{weekId:long}/video")]
    [FeatureAuthorize("PROGRAM_WEEK_UPDATE")]
    public async Task<ActionResult> DeleteVideo(
        Int64 programId,
        Int64 weekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting video for program {ProgramId} week {WeekId}", programId, weekId);
        await _mediator.Send(new DeleteVideoCommand(programId, weekId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a program week by its ID.
    /// </summary>
    /// <returns>No content.</returns>
    [HttpDelete("{weekId:long}")]
    [FeatureAuthorize("PROGRAM_WEEK_DELETE")]
    public async Task<IActionResult> Delete(
        Int64 programId,
        Int64 weekId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting program week {WeekId} for program {ProgramId}", weekId, programId);
        await _mediator.Send(
            new DeleteProgramWeekCommand(programId, weekId), cancellationToken);
        return NoContent();
    }
}

