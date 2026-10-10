using IdentityService.Application.Contracts;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Users.Commands;

/// <summary>
/// Command to delete a user by their unique identifier.
/// </summary>
public sealed record DeleteUserCommand(Guid Id) : IRequest<String>;

/// <summary>
/// Handles the <see cref="DeleteUserCommand"/> by removing the user from the database.
/// </summary>
public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, String>
{
    private readonly IIdentityService _identityService;
    private readonly IUserContextService _userContext;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserCommandHandler"/> class.
    /// </summary>
    public DeleteUserCommandHandler(
        IIdentityService identityService,
        IUserContextService userContext,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _identityService = identityService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates authorization, and deletes the user.
    /// </summary>
    /// <returns>A success message.</returns>
    public async Task<String> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        Guid id = request.Id;

        _logger.LogInformation("Deleting user: {UserId}", id);

        if (_userContext.UserId is null)
        {
            _logger.LogWarning("Delete user failed - user not authenticated");
            throw new UnauthorizedException("User is not authenticated.");
        }

        if (!_userContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Delete user failed - not an admin role");
            throw new ForbiddenException("Only admins can delete users.");
        }

        User? targetUser = await _identityService.GetUserByIdAsync(id);
        if (targetUser is null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            throw new NotFoundException("User", id);
        }

        if (_userContext.OrganizationId != targetUser.OrganizationId)
        {
            _logger.LogWarning("Delete user failed - organization mismatch");
            throw new ForbiddenException("You are not authorized to delete users from another organization.");
        }

        _identityService.DeleteUser(targetUser);
        await _identityService.SaveAsync();

        return "User deleted successfully";
    }
}
