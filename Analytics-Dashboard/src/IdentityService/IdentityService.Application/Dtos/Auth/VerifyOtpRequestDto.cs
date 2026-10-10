namespace IdentityService.Application.Dtos.Auth;

/// <summary>
/// Data transfer object for an OTP verification request.
/// </summary>
public sealed class VerifyOtpRequestDto
{
    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the OTP to verify.</summary>
    public string Otp { get; set; } = string.Empty;
}
