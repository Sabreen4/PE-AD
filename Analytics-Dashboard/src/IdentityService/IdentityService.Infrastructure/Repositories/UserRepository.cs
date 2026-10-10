using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing User entities in the database.
/// </summary>
public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    public UserRepository(IdentityDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
    }
}
