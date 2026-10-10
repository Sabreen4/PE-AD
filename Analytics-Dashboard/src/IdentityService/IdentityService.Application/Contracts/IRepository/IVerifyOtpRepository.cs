using IdentityService.Domain.Entities;
using Shared.Common.Interfaces;

namespace IdentityService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for managing VerifyOtp (OTP verification) entities.
/// </summary>
public interface IVerifyOtpRepository : IRepositoryBase<VerifyOtp>
{
}
