namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object containing JWT access and refresh tokens.
/// </summary>
public sealed class TokenResponseDto
{
    /// <summary>Gets or sets the JWT access token.</summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>Gets or sets the refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
