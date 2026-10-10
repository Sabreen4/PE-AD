namespace EvaluationService.Application.Dtos.Leaderboard;

/// <summary>
/// DTO representing a single leaderboard entry with rank and score.
/// </summary>
public class LeaderboardEntryDto
{
    public Int64 Id { get; set; }
    public Int64 EnrollmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal OverallScore { get; set; }
    public int Rank { get; set; }
    public decimal Percentile { get; set; }
}
