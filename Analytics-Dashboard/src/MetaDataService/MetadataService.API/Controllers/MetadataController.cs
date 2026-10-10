using Microsoft.AspNetCore.Mvc;
using MetaDataService.Application.Dtos;
using MetaDataService.Application.Contracts;
using Shared.Authorization.Attributes;

namespace MetaDataService.API.Controllers;

[ApiController]
[Route("metadata")]
[FeatureAuthorize("FEEDBACK_PROVIDE")]
/// <summary>
/// REST controller for general metadata lookups.
/// </summary>
public sealed class MetadataController : ControllerBase
{
    private readonly IMetadataService _metadataService;
    private readonly ILogger<MetadataController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataController"/> class.
    /// </summary>
    /// <param name="metadataService">The metadata application service.</param>
    /// <param name="logger">The logger instance.</param>
    public MetadataController(IMetadataService metadataService, ILogger<MetadataController> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the list of available file types from the "FileType" RefSet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of file type DTOs.</returns>
    [HttpGet("file-types")]
    public async Task<ActionResult<List<FileTypeResponseDto>>> GetFileTypes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting file types");
        Shared.Common.Dtos.PagedResponseDto<RefSetTermResponseDto> result = await _metadataService.GetRefSetTermsAsync("FileType", 1, int.MaxValue, true, null, cancellationToken);

        List<FileTypeResponseDto> items = result.Data
            .Select(t => new FileTypeResponseDto { Key = t.Key, DisplayName = t.Value })
            .ToList();

        return Ok(items);
    }
}
