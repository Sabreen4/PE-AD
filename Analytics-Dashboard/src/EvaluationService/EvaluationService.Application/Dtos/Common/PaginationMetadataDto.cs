using System.Text.Json.Serialization;

namespace EvaluationService.Application.Dtos.Common;

/// <summary>
/// DTO carrying pagination metadata such as page number, size, count, and total pages.
/// </summary>
public class PaginationMetadataDto
{
    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    /// <summary>
    /// Creates a new <see cref="PaginationMetadataDto"/> with computed pagination values.
    /// </summary>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">The total number of records.</param>
    /// <returns>A populated <see cref="PaginationMetadataDto"/>.</returns>
    public static PaginationMetadataDto Create(int pageNumber, int pageSize, int totalRecords) =>
        new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            PageCount = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
}
