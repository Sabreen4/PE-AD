using EvaluationService.Application.Dtos.Common;

namespace EvaluationService.Application.Dtos.WeeklyEvaluation;

/// <summary>
/// Response DTO containing a paginated list of weekly evaluations.
/// </summary>
public class WeeklyEvaluationListResponseDto
{
    public List<WeeklyEvaluationDto> Data { get; set; } = new();
    public PaginationMetadataDto Pagination { get; set; } = default!;
}
