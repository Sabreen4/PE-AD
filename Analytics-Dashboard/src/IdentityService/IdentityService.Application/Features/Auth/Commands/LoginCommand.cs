using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Auth;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Interfaces;

namespace IdentityService.Application.Features.Auth.Commands;

/// <summary>
/// Command to initiate a login by sending an OTP to the user's email.
/// </summary>
public sealed record LoginCommand(LoginRequestDto Dto) : IRequest<LoginResponseDto>;

/// <summary>
/// Handles the <see cref="LoginCommand"/> by validating the user and sending an OTP via email.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IOtpService _otpService;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    public LoginCommandHandler(
        IIdentityService identityService,
        IOtpService otpService,
        INotificationGrpcClient notificationClient,
        ILogger<LoginCommandHandler> logger)
    {
        _identityService = identityService;
        _otpService = otpService;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates the user, generates an OTP, and sends it via email.
    /// </summary>
    /// <returns>A login response DTO containing the OTP expiry and message.</returns>
    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        LoginRequestDto dto = request.Dto;

        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        User? user = await _identityService.GetUserByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", dto.Email);
            throw new NotFoundException("User not found with this email");
        }

        String otp = _otpService.GenerateOtp();
        DateTime expiry = DateTime.UtcNow.AddSeconds(_otpService.ExpirySeconds);

        VerifyOtp? existing = await _identityService.GetPendingOtpByUserAsync(user.Id);
        if (existing is not null)
        {
            existing.Otp = otp;
            existing.ExpiryDate = expiry;
            existing.IsVerified = false;
            _identityService.UpdateVerifyOtp(existing);
        }
        else
        {
            VerifyOtp verifyOtp = new VerifyOtp
            {
                UserId = user.Id,
                Otp = otp,
                ExpiryDate = expiry,
                IsVerified = false
            };
            await _identityService.CreateVerifyOtpAsync(verifyOtp);
        }

        await _identityService.SaveAsync();

        await _notificationClient.SendOtpEmailAsync(user.Email, otp, _otpService.ExpirySeconds / 60);

        return new LoginResponseDto
        {
            OtpExpiry = _otpService.ExpirySeconds,
            Message = "OTP sent to your email. Please verify within 10 minutes."
        };
    }
}
