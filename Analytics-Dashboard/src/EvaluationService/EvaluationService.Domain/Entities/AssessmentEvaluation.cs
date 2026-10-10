using Shared.Common.Entities;

namespace EvaluationService.Domain.Entities;

/// <summary>
/// Represents an assessment evaluation score for an employee assignment.
/// </summary>
public class AssessmentEvaluation : BaseEntity
{
    public long Id { get; set; }

    public long EmployeeAssignmentId { get; set; }

    public decimal Score { get; set; }

    public string Remarks { get; set; } = string.Empty;
}
