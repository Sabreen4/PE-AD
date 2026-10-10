using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Auth;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Auth.Commands;

/// <summary>
/// Command to verify an OTP and issue JWT access and refresh tokens.
/// </summary>
public sealed record VerifyOtpCommand(VerifyOtpRequestDto Dto) : IRequest<TokenResponseDto>;

/// <summary>
/// Handles the <see cref="VerifyOtpCommand"/> by validating the OTP and issuing tokens.
/// </summary>
public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, TokenResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<VerifyOtpCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyOtpCommandHandler"/> class.
    /// </summary>
    public VerifyOtpCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ILogger<VerifyOtpCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates the OTP, and issues JWT access and refresh tokens.
    /// </summary>
    /// <returns>A token response DTO with access and refresh tokens.</returns>
    public async Task<TokenResponseDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        VerifyOtpRequestDto dto = request.Dto;

        _logger.LogInformation("OTP verification for email: {Email}", dto.Email);

        User? user = await _identityService.GetUserByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("OTP verification failed - user not found: {Email}", dto.Email);
            throw new NotFoundException("User not found");
        }

        VerifyOtp? otpRecord = await _identityService.GetVerifyOtpAsync(user.Id, dto.Otp);
        if (otpRecord is null)
        {
            _logger.LogWarning("OTP verification failed - invalid OTP for email: {Email}", dto.Email);
            throw new UnauthorizedException("Invalid OTP");
        }

        if (otpRecord.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogWarning("OTP has expired for email: {Email}", dto.Email);
            _identityService.DeleteVerifyOtp(otpRecord);
            await _identityService.SaveAsync();
            throw new UnauthorizedException("OTP has expired. Please request a new one.");
        }

        otpRecord.IsVerified = true;
        _identityService.UpdateVerifyOtp(otpRecord);
        await _identityService.SaveAsync();

        String accessToken = _tokenService.GenerateAccessToken(user.Id, user.Role.Name, user.Email, user.OrganizationId);
        String refreshToken = _tokenService.GenerateRefreshToken();

        RefreshToken refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _identityService.CreateRefreshTokenAsync(refreshTokenEntity);
        await _identityService.SaveAsync();

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
