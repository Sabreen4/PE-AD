using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Domain.Entities;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="WeeklyEvaluation"/> entities.
/// </summary>
public sealed class WeeklyEvaluationRepository : RepositoryBase<WeeklyEvaluation>, IWeeklyEvaluationRepository
{
    public WeeklyEvaluationRepository(EvaluationDbContext context, ILogger<WeeklyEvaluationRepository> logger)
        : base(context, logger)
    {
    }
}
