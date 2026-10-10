using EvaluationService.Application.Dtos.Common;

namespace EvaluationService.Application.Dtos.Leaderboard;

/// <summary>
/// Response DTO containing a paginated list of leaderboard entries.
/// </summary>
public class LeaderboardListResponseDto
{
    public List<LeaderboardEntryDto> Data { get; set; } = new();
    public PaginationMetadataDto Pagination { get; set; } = default!;
}
