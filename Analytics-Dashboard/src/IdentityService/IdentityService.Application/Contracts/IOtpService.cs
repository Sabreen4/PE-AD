namespace IdentityService.Application.Contracts;

/// <summary>
/// Defines the contract for generating one-time passwords (OTPs).
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP.
    /// </summary>
    /// <returns>The generated OTP string.</returns>
    string GenerateOtp();
    /// <summary>Gets the OTP expiry duration in seconds.</summary>
    int ExpirySeconds { get; }
}
