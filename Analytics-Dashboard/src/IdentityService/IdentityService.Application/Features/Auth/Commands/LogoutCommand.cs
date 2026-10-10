using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Auth;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Auth.Commands;

/// <summary>
/// Command to log out a user by revoking their refresh token.
/// </summary>
public sealed record LogoutCommand(LogoutRequestDto Dto) : IRequest<String>;

/// <summary>
/// Handles the <see cref="LogoutCommand"/> by revoking the refresh token.
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, String>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutCommandHandler"/> class.
    /// </summary>
    public LogoutCommandHandler(IIdentityService identityService, ILogger<LogoutCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command and revokes the refresh token.
    /// </summary>
    /// <returns>A success message indicating logout was successful.</returns>
    public async Task<String> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        LogoutRequestDto dto = request.Dto;

        _logger.LogInformation("Logout request received");

        RefreshToken? storedToken = await _identityService.GetRefreshTokenAsync(dto.RefreshToken);
        if (storedToken is null)
        {
            _logger.LogWarning("Logout failed - invalid refresh token");
            throw new UnauthorizedException("Invalid refresh token");
        }

        storedToken.IsRevoked = true;
        _identityService.UpdateRefreshToken(storedToken);
        await _identityService.SaveAsync();

        return "Logged out successfully.";
    }
}
