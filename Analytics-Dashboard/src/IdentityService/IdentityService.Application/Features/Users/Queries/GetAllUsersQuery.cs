using AutoMapper;
using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Users.Queries;

/// <summary>
/// Query to retrieve all users for the current user's organization.
/// </summary>
public sealed record GetAllUsersQuery : IRequest<List<UserResponseDto>>;

/// <summary>
/// Handles the <see cref="GetAllUsersQuery"/> and returns a list of user response DTOs.
/// </summary>
public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IUserContextService _userContext;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersQueryHandler"/> class.
    /// </summary>
    public GetAllUsersQueryHandler(
        IIdentityService identityService,
        IUserContextService userContext,
        IMapper mapper,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _identityService = identityService;
        _userContext = userContext;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns all users for the current organization.
    /// </summary>
    /// <returns>A list of user response DTOs.</returns>
    public async Task<List<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all users");

        if (_userContext.UserId is null)
        {
            _logger.LogWarning("Get all users failed - user not authenticated");
            throw new UnauthorizedException("User is not authenticated.");
        }

        List<User> users = await _identityService.GetUsersByOrganizationAsync(_userContext.OrganizationId);
        return _mapper.Map<List<UserResponseDto>>(users);
    }
}
