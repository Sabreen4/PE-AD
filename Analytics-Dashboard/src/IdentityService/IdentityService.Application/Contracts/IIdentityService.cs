using IdentityService.Domain.Entities;

namespace IdentityService.Application.Contracts;

/// <summary>
/// Defines the contract for identity domain operations including organizations, users, roles, refresh tokens, and OTP verification.
/// </summary>
public interface IIdentityService
{
    /// <summary>Retrieves all organizations.</summary>
    Task<List<Organization>> GetAllOrganizationsAsync();
    /// <summary>Retrieves an organization by its unique identifier.</summary>
    Task<Organization?> GetOrganizationByIdAsync(long id);
    /// <summary>Checks whether an organization with the given name exists.</summary>
    Task<bool> OrganizationNameExistsAsync(string name);
    /// <summary>Checks whether an organization with the given email exists.</summary>
    Task<bool> OrganizationEmailExistsAsync(string email);
    /// <summary>Creates a new organization.</summary>
    Task CreateOrganizationAsync(Organization organization);
    /// <summary>Updates an existing organization.</summary>
    void UpdateOrganization(Organization organization);
    /// <summary>Deletes an organization.</summary>
    void DeleteOrganization(Organization organization);
    /// <summary>Retrieves all users.</summary>
    Task<List<User>> GetAllUsersAsync();
    /// <summary>Retrieves a user by their unique identifier.</summary>
    Task<User?> GetUserByIdAsync(Guid id);
    /// <summary>Retrieves a user by their email address.</summary>
    Task<User?> GetUserByEmailAsync(string email);
    /// <summary>Checks whether a user with the given email exists.</summary>
    Task<bool> EmailExistsAsync(string email);
    /// <summary>Creates a new user.</summary>
    Task CreateUserAsync(User user);
    /// <summary>Updates an existing user.</summary>
    void UpdateUser(User user);
    /// <summary>Deletes a user.</summary>
    void DeleteUser(User user);
    /// <summary>Retrieves users belonging to a specific organization.</summary>
    Task<List<User>> GetUsersByOrganizationAsync(long organizationId);
    /// <summary>Retrieves a role by its name.</summary>
    Task<Role?> GetRoleByNameAsync(string name);
    /// <summary>Retrieves a non-revoked refresh token by its value.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    /// <summary>Creates a new refresh token.</summary>
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);
    /// <summary>Updates an existing refresh token.</summary>
    void UpdateRefreshToken(RefreshToken refreshToken);
    /// <summary>Deletes a refresh token.</summary>
    void DeleteRefreshToken(RefreshToken refreshToken);
    /// <summary>Retrieves a valid OTP record for a user.</summary>
    Task<VerifyOtp?> GetVerifyOtpAsync(Guid userId, string otp);
    /// <summary>Retrieves a pending OTP record for a user.</summary>
    Task<VerifyOtp?> GetPendingOtpByUserAsync(Guid userId);
    /// <summary>Creates a new OTP verification record.</summary>
    Task CreateVerifyOtpAsync(VerifyOtp verifyOtp);
    /// <summary>Updates an existing OTP verification record.</summary>
    void UpdateVerifyOtp(VerifyOtp verifyOtp);
    /// <summary>Deletes an OTP verification record.</summary>
    void DeleteVerifyOtp(VerifyOtp verifyOtp);
    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveAsync();
}
