using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing Organization entities.
/// </summary>
public interface IOrganizationRepository : IRepositoryBase<Organization>
{
}
