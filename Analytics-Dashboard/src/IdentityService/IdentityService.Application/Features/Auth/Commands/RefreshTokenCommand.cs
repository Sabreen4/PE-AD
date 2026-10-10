using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Auth;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Auth.Commands;

/// <summary>
/// Command to refresh a JWT access token using a valid refresh token.
/// </summary>
public sealed record RefreshTokenCommand(RefreshTokenRequestDto Dto) : IRequest<TokenResponseDto>;

/// <summary>
/// Handles the <see cref="RefreshTokenCommand"/> by validating the refresh token and issuing a new access token.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
    /// </summary>
    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates the refresh token, and generates a new access token.
    /// </summary>
    /// <returns>A token response DTO with the new access token.</returns>
    public async Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        RefreshTokenRequestDto dto = request.Dto;

        _logger.LogInformation("Refresh token request received");

        RefreshToken? storedToken = await _identityService.GetRefreshTokenAsync(dto.RefreshToken);
        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token failed - invalid token");
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token has expired");
            storedToken.IsRevoked = true;
            _identityService.UpdateRefreshToken(storedToken);
            await _identityService.SaveAsync();
            throw new UnauthorizedException("Refresh token has expired");
        }

        User user = storedToken.User;

        String newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Role.Name, user.Email, user.OrganizationId);

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = dto.RefreshToken
        };
    }
}
