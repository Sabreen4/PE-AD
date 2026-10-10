namespace Shared.Common.Dtos;

/// <summary>
/// Standard JSON shape for a paged list result, combining the data with pagination metadata.
/// </summary>
public class PagedResponseDto<T> : SuccessResponseDto<IEnumerable<T>>
{
    /// <summary>Pagination metadata for the current page of results.</summary>
    public PaginationDto Pagination { get; set; } = default!;
}
