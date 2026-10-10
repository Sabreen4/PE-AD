using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Users.Commands;

/// <summary>
/// Command to update a user's profile information.
/// </summary>
public sealed record UpdateUserCommand(Guid Id, UpdateUserRequestDto Dto) : IRequest<IdResponseDto>;

/// <summary>
/// Handles the <see cref="UpdateUserCommand"/> by updating the user in the database.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, IdResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUserContextService _userContext;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserCommandHandler"/> class.
    /// </summary>
    public UpdateUserCommandHandler(
        IIdentityService identityService,
        IUserContextService userContext,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _identityService = identityService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates authorization, and updates the user profile.
    /// </summary>
    /// <returns>An ID response DTO with the user ID.</returns>
    public async Task<IdResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        Guid id = request.Id;
        UpdateUserRequestDto dto = request.Dto;

        _logger.LogInformation("Updating user: {UserId}", id);

        if (_userContext.UserId is null)
        {
            _logger.LogWarning("Update user failed - user not authenticated");
            throw new UnauthorizedException("User is not authenticated.");
        }

        User? targetUser = await _identityService.GetUserByIdAsync(id);
        if (targetUser is null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            throw new NotFoundException("User", id);
        }

        if (_userContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            if (_userContext.OrganizationId != targetUser.OrganizationId)
                throw new ForbiddenException("You are not authorized to update users from another organization.");
        }
        else if (_userContext.Roles.Contains("Employee", StringComparer.OrdinalIgnoreCase)
              || _userContext.Roles.Contains("Evaluator", StringComparer.OrdinalIgnoreCase))
        {
            if (_userContext.UserId.Value != id)
                throw new ForbiddenException("You can only update your own profile.");
        }
        else
        {
            throw new ForbiddenException("You do not have permission to update users.");
        }

        targetUser.FirstName = dto.FirstName;
        targetUser.LastName = dto.LastName;
        targetUser.Phone = dto.Phone;

        _identityService.UpdateUser(targetUser);
        await _identityService.SaveAsync();

        return new IdResponseDto
        {
            Id = id,
            Message = "User updated successfully"
        };
    }
}
