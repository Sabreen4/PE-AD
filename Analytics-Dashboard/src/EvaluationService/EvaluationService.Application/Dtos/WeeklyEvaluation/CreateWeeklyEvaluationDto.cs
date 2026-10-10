namespace EvaluationService.Application.Dtos.WeeklyEvaluation;

/// <summary>
/// DTO for creating a new weekly evaluation.
/// </summary>
public class CreateWeeklyEvaluationDto
{
    public long ProgramWeekId { get; set; }

    public long EmployeeAssignmentId { get; set; }

    public int EngagementScore { get; set; }

    public int ApplicationScore { get; set; }
}
