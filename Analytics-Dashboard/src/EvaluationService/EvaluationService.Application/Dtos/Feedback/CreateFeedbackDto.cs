namespace EvaluationService.Application.Dtos.Feedback;

/// <summary>
/// DTO for creating a new feedback entry.
/// </summary>
public class CreateFeedbackDto
{
    public Guid EmployeeId { get; set; }

    public long WeeklyEvaluationId { get; set; }

    public string FeedbackTypeCode { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;
}
