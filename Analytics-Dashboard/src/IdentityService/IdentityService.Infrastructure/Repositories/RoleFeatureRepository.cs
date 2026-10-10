using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing RoleFeature (junction) entities in the database.
/// </summary>
public sealed class RoleFeatureRepository : RepositoryBase<RoleFeature>, IRoleFeatureRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleFeatureRepository"/> class.
    /// </summary>
    public RoleFeatureRepository(IdentityDbContext context, ILogger<RoleFeatureRepository> logger) : base(context, logger)
    {
    }
}
