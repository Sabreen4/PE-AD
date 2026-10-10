using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Organization entities in the database.
/// </summary>
public sealed class OrganizationRepository : RepositoryBase<Organization>, IOrganizationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationRepository"/> class.
    /// </summary>
    public OrganizationRepository(IdentityDbContext context, ILogger<OrganizationRepository> logger) : base(context, logger)
    {
    }
}
