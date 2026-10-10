using Shared.Common.Entities;

namespace EvaluationService.Domain.Entities;

/// <summary>
/// Represents feedback (green flag or red flag) given to an employee for a weekly evaluation.
/// </summary>
public sealed class Feedback : BaseEntity
{
    public long Id { get; set; }

    public long WeeklyEvaluationId { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid FeedbackTypeId { get; set; }

    public string FeedbackTypeCode { get; set; } = string.Empty;

    public string FeedbackTypeName { get; set; } = string.Empty;

    public decimal FeedbackScoreAdjustment { get; set; }

    public string Comment { get; set; } = string.Empty;

    public WeeklyEvaluation WeeklyEvaluation { get; set; } = null!;
}
