using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing RefreshToken entities.
/// </summary>
public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
}
