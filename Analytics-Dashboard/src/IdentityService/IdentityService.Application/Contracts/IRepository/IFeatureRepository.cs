using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing Feature entities.
/// </summary>
public interface IFeatureRepository : IRepositoryBase<Feature>
{
}
