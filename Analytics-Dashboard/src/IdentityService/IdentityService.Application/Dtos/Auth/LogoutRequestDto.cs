namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object for a logout request containing the refresh token to revoke.
/// </summary>
public sealed class LogoutRequestDto
{
    /// <summary>Gets or sets the refresh token to revoke.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
