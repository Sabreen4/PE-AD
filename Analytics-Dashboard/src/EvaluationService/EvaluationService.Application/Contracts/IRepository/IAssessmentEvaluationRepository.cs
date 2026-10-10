using EvaluationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace EvaluationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for <see cref="AssessmentEvaluation"/> entities.
/// </summary>
public interface IAssessmentEvaluationRepository : IRepositoryBase<AssessmentEvaluation>
{
}
