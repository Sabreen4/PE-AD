using EvaluationService.Application.Dtos.Common;

namespace EvaluationService.Application.Dtos.Feedback;

/// <summary>
/// Response DTO containing a paginated list of feedback entries.
/// </summary>
public class FeedbackListResponseDto
{
    public List<FeedbackDto> Data { get; set; } = new();
    public PaginationMetadataDto Pagination { get; set; } = default!;
}
