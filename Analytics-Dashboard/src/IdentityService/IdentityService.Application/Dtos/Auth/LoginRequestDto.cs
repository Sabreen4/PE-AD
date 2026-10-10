namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object for a login request containing the user's email.
/// </summary>
public sealed class LoginRequestDto
{
    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;
}
