using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Role entities in the database.
/// </summary>
public sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </summary>
    public RoleRepository(IdentityDbContext context, ILogger<RoleRepository> logger) : base(context, logger)
    {
    }
}
