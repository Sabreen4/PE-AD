using IdentityService.Application.Contracts.IRepository;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Shared.Common.Repositories;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing VerifyOtp (OTP verification) entities in the database.
/// </summary>
public sealed class VerifyOtpRepository : RepositoryBase<VerifyOtp>, IVerifyOtpRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyOtpRepository"/> class.
    /// </summary>
    public VerifyOtpRepository(IdentityDbContext context, ILogger<VerifyOtpRepository> logger) : base(context, logger)
    {
    }
}
