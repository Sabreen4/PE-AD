using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Feature entities in the database.
/// </summary>
public sealed class FeatureRepository : RepositoryBase<Feature>, IFeatureRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureRepository"/> class.
    /// </summary>
    public FeatureRepository(IdentityDbContext context, ILogger<FeatureRepository> logger) : base(context, logger)
    {
    }
}
