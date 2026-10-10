using MediatR;
using MetaDataService.Application.Contracts;
using MetaDataService.Application.Dtos;
using Microsoft.Extensions.Logging;
using Shared.Common.Dtos;

namespace MetaDataService.Application.Features.RefSets.Queries;

/// <summary>
/// Query to retrieve paginated RefSet terms for a given key.
/// </summary>
public sealed record GetRefSetTermsQuery(
    string Key,
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    string? Search = null
) : IRequest<PagedResponseDto<RefSetTermResponseDto>>;

/// <summary>
/// Handles the <see cref="GetRefSetTermsQuery"/> by delegating to the metadata service.
/// </summary>
public sealed class GetRefSetTermsHandler : IRequestHandler<GetRefSetTermsQuery, PagedResponseDto<RefSetTermResponseDto>>
{
    private readonly IMetadataService _metadataService;
    private readonly ILogger<GetRefSetTermsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRefSetTermsHandler"/> class.
    /// </summary>
    /// <param name="metadataService">The metadata application service.</param>
    /// <param name="logger">The logger instance.</param>
    public GetRefSetTermsHandler(IMetadataService metadataService, ILogger<GetRefSetTermsHandler> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by fetching RefSet terms from the metadata service.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response of RefSet term DTOs.</returns>
    public async Task<PagedResponseDto<RefSetTermResponseDto>> Handle(GetRefSetTermsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ref set terms for key: {Key}, page: {Page}, pageSize: {PageSize}", request.Key, request.Page, request.PageSize);
        return await _metadataService.GetRefSetTermsAsync(
            request.Key, request.Page, request.PageSize, request.IsActive, request.Search, cancellationToken);
    }
}
