namespace EvaluationService.Application.Dtos.WeeklyEvaluation;

/// <summary>
/// DTO representing a weekly evaluation record.
/// </summary>
public class WeeklyEvaluationDto
{
    public Int64 Id { get; set; }
    public Int64 ProgramWeekId { get; set; }
    public Int64 EmployeeAssignmentId { get; set; }
    public decimal AttendanceScore { get; set; }
    public int EngagementScore { get; set; }
    public int ApplicationScore { get; set; }
    public decimal WeeklyAverage { get; set; }
    public decimal AttendancePercentage { get; set; }
}
