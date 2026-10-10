using AutoMapper;
using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Users.Queries;

/// <summary>
/// Query to retrieve a user by their unique identifier.
/// </summary>
public sealed record GetUserQuery(Guid Id) : IRequest<UserResponseDto>;

/// <summary>
/// Handles the <see cref="GetUserQuery"/> and returns the user response DTO.
/// </summary>
public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserQueryHandler"/> class.
    /// </summary>
    public GetUserQueryHandler(IIdentityService identityService, IMapper mapper, ILogger<GetUserQueryHandler> logger)
    {
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns the user DTO.
    /// </summary>
    /// <returns>The user response DTO.</returns>
    public async Task<UserResponseDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user: {UserId}", request.Id);

        User? user = await _identityService.GetUserByIdAsync(request.Id);
        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", request.Id);
            throw new NotFoundException("User", request.Id);
        }

        return _mapper.Map<UserResponseDto>(user);
    }
}
