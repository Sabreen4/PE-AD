using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Domain.Entities;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="Feedback"/> entities.
/// </summary>
public sealed class FeedbackRepository : RepositoryBase<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(EvaluationDbContext context, ILogger<FeedbackRepository> logger)
        : base(context, logger)
    {
    }
}
