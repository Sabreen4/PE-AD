namespace EvaluationService.Application.Dtos.Feedback;

/// <summary>
/// DTO for updating an existing feedback entry.
/// </summary>
public class UpdateFeedbackDto
{
    public string FeedbackTypeCode { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;
}
