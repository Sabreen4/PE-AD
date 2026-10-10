using MetaDataService.Application.Dtos;
using Shared.Common.Dtos;

namespace MetaDataService.Application.Contracts;

/// <summary>
/// Application service contract for retrieving reference data.
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Retrieves paginated RefSet terms by the given key with optional active filtering.
    /// </summary>
    /// <param name="key">The RefSet key (name).</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="isActive">Optional filter for active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing RefSet term DTOs.</returns>
    Task<PagedResponseDto<RefSetTermResponseDto>> GetRefSetTermsAsync(
        string key, int page = 1, int pageSize = 20, bool? isActive = null, string? search = null, CancellationToken cancellationToken = default);
}
