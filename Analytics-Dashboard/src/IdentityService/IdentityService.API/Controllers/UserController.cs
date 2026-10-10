using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Users;
using IdentityService.Application.Features.Users.Commands;
using IdentityService.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("user")]
/// <summary>
/// Handles HTTP requests for user CRUD operations.
/// </summary>
public  class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all users for the current user's organization.
    /// </summary>
    [HttpGet]
    [Authorize]
    [FeatureAuthorize("USER_MANAGE", "USER_VIEW")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all users");
        List<UserResponseDto> result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user by id {UserId}", id);
        UserResponseDto result = await _mediator.Send(new GetUserQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new user (employee or evaluator) within the current organization.
    /// </summary>
    [HttpPost]
    [Authorize]
    [FeatureAuthorize("USER_MANAGE")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with email {UserEmail}", dto.Email);
        IdResponseDto result = await _mediator.Send(new CreateUserCommand(dto), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Updates an existing user's profile information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user {UserId}", id);
        IdResponseDto result = await _mediator.Send(new UpdateUserCommand(id, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [FeatureAuthorize("USER_MANAGE")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user {UserId}", id);
        String result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return Ok(result);
    }
}
