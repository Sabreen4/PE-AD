using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Domain.Entities;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="Leaderboard"/> entities.
/// </summary>
public sealed class LeaderboardRepository : RepositoryBase<Leaderboard>, ILeaderboardRepository
{
    public LeaderboardRepository(EvaluationDbContext context, ILogger<LeaderboardRepository> logger)
        : base(context, logger)
    {
    }
}
