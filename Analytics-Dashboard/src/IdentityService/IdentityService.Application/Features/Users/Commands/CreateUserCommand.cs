using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace IdentityService.Application.Features.Users.Commands;

/// <summary>
/// Command to create a new user (employee or evaluator) within the current organization.
/// </summary>
public sealed record CreateUserCommand(CreateUserRequestDto Dto) : IRequest<IdResponseDto>;

/// <summary>
/// Handles the <see cref="CreateUserCommand"/> by creating the user and sending a registration email.
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, IdResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUserContextService _userContext;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserCommandHandler"/> class.
    /// </summary>
    public CreateUserCommandHandler(
        IIdentityService identityService,
        IUserContextService userContext,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        ILogger<CreateUserCommandHandler> logger)
    {
        _identityService = identityService;
        _userContext = userContext;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates inputs and authorization, creates the user, and sends a registration email.
    /// </summary>
    /// <returns>An ID response DTO with the new user ID.</returns>
    public async Task<IdResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        CreateUserRequestDto dto = request.Dto;

        _logger.LogInformation("Creating user with email: {Email}", dto.Email);

        if (_userContext.UserId is null)
        {
            _logger.LogWarning("Create user failed - user not authenticated");
            throw new UnauthorizedException("User is not authenticated.");
        }

        if (!_userContext.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Create user failed - not an admin role");
            throw new ForbiddenException("Only admins can create users.");
        }

        if (_userContext.OrganizationId != dto.OrganizationId)
        {
            _logger.LogWarning("Create user failed - organization mismatch");
            throw new ForbiddenException("You are not authorized to create users for another organization.");
        }

        String normalizedRole = dto.RoleType.ToLower();
        if (normalizedRole != "employee" && normalizedRole != "evaluator")
        {
            _logger.LogWarning("Invalid role type: {RoleType}", dto.RoleType);
            throw new BadRequestException("Admin can only create Employee or Evaluator accounts.");
        }

        Boolean emailExists = await _identityService.EmailExistsAsync(dto.Email);
        if (emailExists)
        {
            _logger.LogWarning("Email already exists: {Email}", dto.Email);
            throw new ConflictException("A user with this email already exists");
        }

        Role? role = await _identityService.GetRoleByNameAsync(normalizedRole);
        if (role is null)
        {
            _logger.LogError("Role not found: {RoleType}", normalizedRole);
            throw new BadRequestException($"Invalid role type: {dto.RoleType}");
        }

        Organization? organization = await _identityService.GetOrganizationByIdAsync(_userContext.OrganizationId);
        if (organization is null)
        {
            _logger.LogWarning("Organization not found: {OrganizationId}", _userContext.OrganizationId);
            throw new NotFoundException("Organization", _userContext.OrganizationId);
        }

        Guid? genderId = null;
        if (!String.IsNullOrWhiteSpace(dto.Gender))
        {
            String normalizedGender = dto.Gender.ToLower();
            RefTermDto? genderTerm = await _metaDataClient.GetByKeyAsync("Gender", normalizedGender, cancellationToken);
            if (genderTerm is null)
            {
                _logger.LogWarning("Invalid gender: {Gender}", dto.Gender);
                throw new BadRequestException($"Invalid gender: {dto.Gender}");
            }
            genderId = Guid.Parse(genderTerm.Id);
        }

        User user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            OrganizationId = _userContext.OrganizationId,
            RoleId = role.Id,
            GenderId = genderId
        };

        await _identityService.CreateUserAsync(user);
        await _identityService.SaveAsync();

        String loginLink = $"https://app.example.com/login?email={dto.Email}";
        await _notificationClient.SendRegistrationEmailAsync(user.Email, user.FirstName, loginLink);

        return new IdResponseDto
        {
            Id = user.Id,
            Message = "User created successfully. Registration email sent."
        };
    }
}
