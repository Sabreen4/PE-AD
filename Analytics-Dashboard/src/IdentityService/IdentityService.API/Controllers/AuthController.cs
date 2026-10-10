using IdentityService.Application.Dtos.Auth;
using IdentityService.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

/// <summary>
/// Handles authentication-related HTTP requests including login, OTP verification, token refresh, and logout.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a login by sending an OTP to the user's email.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User login attempt for email {Email}", dto.Email);
        LoginResponseDto result = await _mediator.Send(new LoginCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verifies the OTP and issues JWT access and refresh tokens.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OTP verification attempt for email {Email}", dto.Email);
        TokenResponseDto result = await _mediator.Send(new VerifyOtpCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes the JWT access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh requested");
        TokenResponseDto result = await _mediator.Send(new RefreshTokenCommand(dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the user by revoking their refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User logout for refresh token {RefreshToken}", dto.RefreshToken);
        String result = await _mediator.Send(new LogoutCommand(dto), cancellationToken);
        return Ok(result);
    }
}
