namespace IdentityService.Application.Contracts;

/// <summary>
/// Defines the contract for generating JWT access tokens and refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <returns>A signed JWT access token string.</returns>
    string GenerateAccessToken(Guid userId, string role, string email, Int64 organizationId);
    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();
}
