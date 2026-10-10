namespace EvaluationService.Application.Dtos.Feedback;

/// <summary>
/// DTO representing a feedback entry with associated evaluation and program details.
/// </summary>
public class FeedbackDto
{
    public long FeedbackId { get; set; }

    public Guid EmployeeId { get; set; }

    public long ProgramId { get; set; }

    public long ProgramWeekId { get; set; }

    public long WeeklyEvaluationId { get; set; }

    public Guid FeedbackTypeId { get; set; }

    public string FeedbackTypeCode { get; set; } = string.Empty;

    public string FeedbackTypeName { get; set; } = string.Empty;

    public decimal FeedbackScoreAdjustment { get; set; }

    public string FeedbackComment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
