using EvaluationService.Application.Dtos.WeeklyEvaluation;

namespace EvaluationService.Application.Dtos.Feedback;

/// <summary>
/// DTO returned after successfully creating a feedback entry.
/// </summary>
public class FeedbackCreatedDto
{
    public long Id { get; set; }

    public FeedbackDto Feedback { get; set; } = null!;
}
