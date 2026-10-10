using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing RoleFeature (junction) entities.
/// </summary>
public interface IRoleFeatureRepository : IRepositoryBase<RoleFeature>
{
}
