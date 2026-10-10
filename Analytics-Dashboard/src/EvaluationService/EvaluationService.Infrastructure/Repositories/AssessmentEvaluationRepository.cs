using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Domain.Entities;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="AssessmentEvaluation"/> entities.
/// </summary>
public sealed class AssessmentEvaluationRepository : RepositoryBase<AssessmentEvaluation>, IAssessmentEvaluationRepository
{
    public AssessmentEvaluationRepository(EvaluationDbContext context, ILogger<AssessmentEvaluationRepository> logger)
        : base(context, logger)
    {
    }
}
