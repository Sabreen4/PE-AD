using IdentityService.Application.Contracts;
using IdentityService.Application.Contracts.IRepository;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;

namespace IdentityService.Infrastructure.Common;

/// <summary>
/// Wraps all repository interfaces into a single unit of work for the Identity Service.
/// </summary>
public sealed class RepoWrapper : IRepoWrapper
{
    private readonly IdentityDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly ILogger<RepoWrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private IOrganizationRepository? _organization;
    private IUserRepository? _user;
    private IRoleRepository? _role;
    private IFeatureRepository? _feature;
    private IRoleFeatureRepository? _roleFeature;
    private IRefreshTokenRepository? _refreshToken;
    private IVerifyOtpRepository? _verifyOtp;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepoWrapper"/> class.
    /// </summary>
    public RepoWrapper(IdentityDbContext context, IUserContextService userContext, ILogger<RepoWrapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Gets the organization repository.</summary>
    public IOrganizationRepository Organization
    {
        get { return _organization ??= new OrganizationRepository(_context, _loggerFactory.CreateLogger<OrganizationRepository>()); }
    }

    /// <summary>Gets the user repository.</summary>
    public IUserRepository User
    {
        get { return _user ??= new UserRepository(_context, _loggerFactory.CreateLogger<UserRepository>()); }
    }

    /// <summary>Gets the role repository.</summary>
    public IRoleRepository Role
    {
        get { return _role ??= new RoleRepository(_context, _loggerFactory.CreateLogger<RoleRepository>()); }
    }

    /// <summary>Gets the feature repository.</summary>
    public IFeatureRepository Feature
    {
        get { return _feature ??= new FeatureRepository(_context, _loggerFactory.CreateLogger<FeatureRepository>()); }
    }

    /// <summary>Gets the role-feature mapping repository.</summary>
    public IRoleFeatureRepository RoleFeature
    {
        get { return _roleFeature ??= new RoleFeatureRepository(_context, _loggerFactory.CreateLogger<RoleFeatureRepository>()); }
    }

    /// <summary>Gets the refresh token repository.</summary>
    public IRefreshTokenRepository RefreshToken
    {
        get { return _refreshToken ??= new RefreshTokenRepository(_context, _loggerFactory.CreateLogger<RefreshTokenRepository>()); }
    }

    /// <summary>Gets the verify OTP repository.</summary>
    public IVerifyOtpRepository VerifyOtp
    {
        get { return _verifyOtp ??= new VerifyOtpRepository(_context, _loggerFactory.CreateLogger<VerifyOtpRepository>()); }
    }

    /// <summary>
    /// Saves all pending changes to the database with audit fields applied.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<Int32> SaveAsync()
    {
        _logger.LogInformation("Saving changes to database");
        Guid currentUserId = _userContext.UserId ?? Guid.Empty;
        int result = await _context.SaveAsync(currentUserId);
        _logger.LogInformation("Saved {Count} changes to database", result);
        return result;
    }
}
