using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing User entities.
/// </summary>
public interface IUserRepository : IRepositoryBase<User>
{
}
