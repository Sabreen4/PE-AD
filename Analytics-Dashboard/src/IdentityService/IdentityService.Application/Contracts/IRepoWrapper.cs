using IdentityService.Application.Contracts.IRepository;

namespace IdentityService.Application.Contracts;

/// <summary>
/// Wraps all repository interfaces into a single unit of work for coordinated database access.
/// </summary>
public interface IRepoWrapper
{
    /// <summary>Gets the organization repository.</summary>
    IOrganizationRepository Organization { get; }
    /// <summary>Gets the user repository.</summary>
    IUserRepository User { get; }
    /// <summary>Gets the role repository.</summary>
    IRoleRepository Role { get; }
    /// <summary>Gets the feature repository.</summary>
    IFeatureRepository Feature { get; }
    /// <summary>Gets the role-feature mapping repository.</summary>
    IRoleFeatureRepository RoleFeature { get; }
    /// <summary>Gets the refresh token repository.</summary>
    IRefreshTokenRepository RefreshToken { get; }
    /// <summary>Gets the verify OTP repository.</summary>
    IVerifyOtpRepository VerifyOtp { get; }
    /// <summary>Saves all pending changes to the database.</summary>
    /// <returns>The number of state entries written.</returns>
    Task<Int32> SaveAsync();
}
