namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object for a refresh token request.
/// </summary>
public sealed class RefreshTokenRequestDto
{
    /// <summary>Gets or sets the refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
