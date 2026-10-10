namespace EvaluationService.Application.Dtos.AssessmentEvaluation;

/// <summary>
/// DTO representing assessment evaluation details for an employee assignment.
/// </summary>
public class AssessmentEvaluationDetailDto
{
    public Int64 Id { get; set; }
    public decimal? Score { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
