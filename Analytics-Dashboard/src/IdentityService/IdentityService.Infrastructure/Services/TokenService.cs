using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Constants;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Service for generating JWT access tokens and secure refresh tokens
/// used for authentication and authorization.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generates a JWT access token for the specified user with role, email, and organization claims.
    /// </summary>
    /// <returns>A signed JWT access token string.</returns>
    public string GenerateAccessToken(Guid userId, String role, String email, Int64 organizationId)
    {
        _logger.LogInformation("Generating access token for user {UserId} with role {Role}", userId, role);

        String key = _configuration["Jwt:Key"]!;
        String issuer = _configuration["Jwt:Issuer"]!;
        String audience = _configuration["Jwt:Audience"]!;
        Int32 expiryMinutes = Int32.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, email),
            new Claim(CustomClaimTypes.OrganizationId, organizationId.ToString())
        };

        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        _logger.LogInformation("Access token generated for user {UserId}, expires in {ExpiryMinutes} minutes", userId, expiryMinutes);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    public String GenerateRefreshToken()
    {
        _logger.LogInformation("Generating refresh token");
        Byte[] randomBytes = new Byte[64];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}
