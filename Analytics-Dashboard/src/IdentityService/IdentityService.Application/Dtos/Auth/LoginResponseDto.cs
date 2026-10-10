namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object for a login response containing OTP expiry details.
/// </summary>
public sealed class LoginResponseDto
{
    /// <summary>Gets or sets the OTP expiry duration in seconds.</summary>
    public int OtpExpiry { get; set; }
    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;
}
