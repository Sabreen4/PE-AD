using IdentityService.Application.Contracts;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services;

/// <summary>
/// Application service handling identity domain operations including
/// organization, user, role, refresh token, and OTP verification management.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly IRepoWrapper _repo;
    private readonly ILogger<IdentityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityService"/> class.
    /// </summary>
    public IdentityService(IRepoWrapper repo, ILogger<IdentityService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    public async Task<List<Organization>> GetAllOrganizationsAsync()
    {
        _logger.LogInformation("Getting all organizations");
        return await _repo.Organization.FindAll().ToListAsync();
    }

    /// <summary>
    /// Retrieves an organization by its unique identifier.
    /// </summary>
    public async Task<Organization?> GetOrganizationByIdAsync(long id)
    {
        _logger.LogInformation("Getting organization by Id={OrganizationId}", id);
        return await _repo.Organization.GetByIdAsync(id);
    }

    /// <summary>
    /// Checks whether an organization with the given name already exists.
    /// </summary>
    public async Task<bool> OrganizationNameExistsAsync(string name)
    {
        _logger.LogInformation("Checking organization name existence: {Name}", name);
        return await _repo.Organization.AnyAsync(o => o.Name == name);
    }

    /// <summary>
    /// Checks whether an organization with the given email already exists.
    /// </summary>
    public async Task<bool> OrganizationEmailExistsAsync(string email)
    {
        _logger.LogInformation("Checking organization email existence: {Email}", email);
        return await _repo.Organization.AnyAsync(o => o.Email == email);
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    public async Task CreateOrganizationAsync(Organization organization)
    {
        _logger.LogInformation("Creating organization: {Name}", organization.Name);
        await _repo.Organization.CreateAsync(organization);
    }

    /// <summary>
    /// Updates an existing organization.
    /// </summary>
    public void UpdateOrganization(Organization organization)
    {
        _logger.LogInformation("Updating organization: Id={OrganizationId}", organization.Id);
        _repo.Organization.Update(organization);
    }

    /// <summary>
    /// Deletes an organization.
    /// </summary>
    public void DeleteOrganization(Organization organization)
    {
        _logger.LogInformation("Deleting organization: Id={OrganizationId}", organization.Id);
        _repo.Organization.Delete(organization);
    }

    /// <summary>
    /// Retrieves all users with their role information.
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Getting all users");
        return await _repo.User.FindAll().Include(u => u.Role).ToListAsync();
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting user by Id={UserId}", id);
        return await _repo.User.FindByCondition(u => u.Id == id).Include(u => u.Role).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        _logger.LogInformation("Getting user by email: {Email}", email);
        return await _repo.User.FindByCondition(u => u.Email == email).Include(u => u.Role).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks whether a user with the given email already exists.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        _logger.LogInformation("Checking email existence: {Email}", email);
        return await _repo.User.AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public async Task CreateUserAsync(User user)
    {
        _logger.LogInformation("Creating user: Email={Email}", user.Email);
        await _repo.User.CreateAsync(user);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public void UpdateUser(User user)
    {
        _logger.LogInformation("Updating user: Id={UserId}", user.Id);
        _repo.User.Update(user);
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    public void DeleteUser(User user)
    {
        _logger.LogInformation("Deleting user: Id={UserId}", user.Id);
        _repo.User.Delete(user);
    }

    /// <summary>
    /// Retrieves all users belonging to a specific organization.
    /// </summary>
    public async Task<List<User>> GetUsersByOrganizationAsync(long organizationId)
    {
        _logger.LogInformation("Getting users for organization {OrganizationId}", organizationId);
        return await _repo.User.FindByCondition(u => u.OrganizationId == organizationId).Include(u => u.Role).ToListAsync();
    }

    /// <summary>
    /// Retrieves a role by its name.
    /// </summary>
    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        _logger.LogInformation("Getting role by name: {Name}", name);
        return await _repo.Role.FindByCondition(r => r.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a non-revoked refresh token by its value.
    /// </summary>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        _logger.LogInformation("Getting refresh token");
        return await _repo.RefreshToken.FindByCondition(rt => rt.Token == token && !rt.IsRevoked).Include(rt => rt.User).ThenInclude(u => u.Role).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new refresh token for a user.
    /// </summary>
    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _logger.LogInformation("Creating refresh token for user {UserId}", refreshToken.UserId);
        await _repo.RefreshToken.CreateAsync(refreshToken);
    }

    /// <summary>
    /// Updates an existing refresh token.
    /// </summary>
    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        _logger.LogInformation("Updating refresh token Id={TokenId}", refreshToken.Id);
        _repo.RefreshToken.Update(refreshToken);
    }

    /// <summary>
    /// Deletes a refresh token.
    /// </summary>
    public void DeleteRefreshToken(RefreshToken refreshToken)
    {
        _logger.LogInformation("Deleting refresh token Id={TokenId}", refreshToken.Id);
        _repo.RefreshToken.Delete(refreshToken);
    }

    /// <summary>
    /// Retrieves a valid (non-verified, non-expired) OTP record for a user and OTP value.
    /// </summary>
    public async Task<VerifyOtp?> GetVerifyOtpAsync(Guid userId, string otp)
    {
        _logger.LogInformation("Getting verify OTP for user {UserId}", userId);
        return await _repo.VerifyOtp
            .FindByCondition(o => o.UserId == userId && o.Otp == otp && !o.IsVerified && o.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a pending (non-verified, non-expired) OTP record for a user.
    /// </summary>
    public async Task<VerifyOtp?> GetPendingOtpByUserAsync(Guid userId)
    {
        _logger.LogInformation("Getting pending OTP for user {UserId}", userId);
        return await _repo.VerifyOtp
            .FindByCondition(o => o.UserId == userId && !o.IsVerified && o.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new OTP verification record.
    /// </summary>
    public async Task CreateVerifyOtpAsync(VerifyOtp verifyOtp)
    {
        _logger.LogInformation("Creating verify OTP for user {UserId}", verifyOtp.UserId);
        await _repo.VerifyOtp.CreateAsync(verifyOtp);
    }

    /// <summary>
    /// Updates an existing OTP verification record.
    /// </summary>
    public void UpdateVerifyOtp(VerifyOtp verifyOtp)
    {
        _logger.LogInformation("Updating verify OTP Id={OtpId}", verifyOtp.Id);
        _repo.VerifyOtp.Update(verifyOtp);
    }

    /// <summary>
    /// Deletes an OTP verification record.
    /// </summary>
    public void DeleteVerifyOtp(VerifyOtp verifyOtp)
    {
        _logger.LogInformation("Deleting verify OTP Id={OtpId}", verifyOtp.Id);
        _repo.VerifyOtp.Delete(verifyOtp);
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of state entries written.</returns>
    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving identity service changes");
        return await _repo.SaveAsync();
    }
}
