using EvaluationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace EvaluationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for <see cref="Leaderboard"/> entities.
/// </summary>
public interface ILeaderboardRepository : IRepositoryBase<Leaderboard>
{
}
