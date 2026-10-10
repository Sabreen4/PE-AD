using EvaluationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace EvaluationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for <see cref="WeeklyEvaluation"/> entities.
/// </summary>
public interface IWeeklyEvaluationRepository : IRepositoryBase<WeeklyEvaluation>
{
}
