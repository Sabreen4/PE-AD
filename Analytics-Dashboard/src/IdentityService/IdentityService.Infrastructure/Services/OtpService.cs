using IdentityService.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Service for generating one-time passwords (OTPs) used in email verification flows.
/// </summary>
public sealed class OtpService : IOtpService
{
    private static readonly Random Random = new();
    private readonly ILogger<OtpService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OtpService"/> class.
    /// </summary>
    public OtpService(ILogger<OtpService> logger)
    {
        _logger = logger;
    }

    /// <summary>Gets the OTP expiry duration in seconds (10 minutes).</summary>
    public int ExpirySeconds => 600;

    /// <summary>
    /// Generates a random 4-digit OTP.
    /// </summary>
    /// <returns>The generated OTP string.</returns>
    public string GenerateOtp()
    {
        string otp = Random.Next(1000, 9999).ToString("D4");
        _logger.LogInformation("OTP generated, expires in {ExpirySeconds} seconds", ExpirySeconds);
        return otp;
    }
}
