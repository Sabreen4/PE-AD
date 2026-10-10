using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing Role entities.
/// </summary>
public interface IRoleRepository : IRepositoryBase<Role>
{
}
