using EvaluationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace EvaluationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for <see cref="Feedback"/> entities.
/// </summary>
public interface IFeedbackRepository : IRepositoryBase<Feedback>
{
}
