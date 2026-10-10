
namespace Shared.Common.Dtos;

/// <summary>
/// Pagination metadata included alongside a paged result set.
/// </summary>
public class PaginationDto
{
    /// <summary>Current page number (1-based).</summary>
    
    public int Page { get; set; }

    /// <summary>Number of records per page.</summary>
   
    public int PageSize { get; set; }

    /// <summary>Total number of records across all pages.</summary>
    
    public int TotalRecords { get; set; }

    /// <summary>Total number of pages.</summary>
    
    public int TotalPages { get; set; }

    /// <summary>
    /// Builds a <see cref="PaginationDto"/>, computing <see cref="TotalPages"/> from the given counts.
    /// </summary>
    /// <returns>A fully populated <see cref="PaginationDto"/> instance.</returns>
    public static PaginationDto Create(int page, int pageSize, int totalRecords) =>
        new()
        {
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
        };
}
