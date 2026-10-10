using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace IdentityService.Application.Features.Organizations.Commands;

/// <summary>
/// Command to create a new organization along with its admin user account.
/// </summary>
public sealed class CreateOrganizationCommand : IRequest<IdResponseDto>
{
    /// <summary>Gets the DTO containing organization creation details.</summary>
    public CreateOrganizationRequestDto Dto { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrganizationCommand"/> class.
    /// </summary>
    public CreateOrganizationCommand(CreateOrganizationRequestDto dto)
    {
        Dto = dto;
    }
}

/// <summary>
/// Handles the <see cref="CreateOrganizationCommand"/> by creating the organization and admin user.
/// </summary>
public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, IdResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<CreateOrganizationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrganizationCommandHandler"/> class.
    /// </summary>
    public CreateOrganizationCommandHandler(
        IIdentityService identityService,
        INotificationGrpcClient notificationClient,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<CreateOrganizationCommandHandler> logger)
    {
        _identityService = identityService;
        _notificationClient = notificationClient;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command, validates inputs, creates the organization and admin user, and sends a registration email.
    /// </summary>
    /// <returns>An ID response DTO with the new organization ID.</returns>
    public async Task<IdResponseDto> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        CreateOrganizationRequestDto dto = request.Dto;

        _logger.LogInformation("Creating organization: {OrganizationName}", dto.OrganizationName);

        Boolean orgEmailExists = await _identityService.OrganizationEmailExistsAsync(dto.OrganizationEmail);
        if (orgEmailExists)
        {
            _logger.LogWarning("Organization email already exists: {OrganizationEmail}", dto.OrganizationEmail);
            throw new ConflictException("An organization with this email already exists");
        }

        Boolean adminEmailExists = await _identityService.EmailExistsAsync(dto.AdminEmail);
        if (adminEmailExists)
        {
            _logger.LogWarning("Admin email already exists: {AdminEmail}", dto.AdminEmail);
            throw new ConflictException($"A user with email {dto.AdminEmail} already exists");
        }

        Role? adminRole = await _identityService.GetRoleByNameAsync("Admin");
        if (adminRole is null)
        {
            _logger.LogError("Admin role not found - Roles table may not be seeded");
            throw new BadRequestException("Admin role not found. Seed the Roles table first.");
        }

        Guid? adminGenderId = null;
        if (!String.IsNullOrWhiteSpace(dto.AdminGender))
        {
            RefTermDto? genderTerm = await _metaDataClient.GetByKeyAsync("Gender", dto.AdminGender, cancellationToken);
            if (genderTerm is null)
            {
                _logger.LogWarning("Invalid admin gender: {AdminGender}", dto.AdminGender);
                throw new BadRequestException($"Invalid admin gender: {dto.AdminGender}");
            }
            adminGenderId = Guid.Parse(genderTerm.Id);
        }

        Organization organization = new Organization
        {
            Name = dto.OrganizationName,
            Email = dto.OrganizationEmail,
            Phone = dto.OrganizationPhone,
            City = dto.City
        };

        User adminUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.AdminFirstName,
            LastName = dto.AdminLastName,
            Email = dto.AdminEmail,
            Phone = dto.AdminPhone,
            Organization = organization,
            RoleId = adminRole.Id,
            GenderId = adminGenderId,
            IsActive = true
        };

        await _identityService.CreateOrganizationAsync(organization);
        await _identityService.CreateUserAsync(adminUser);
        await _identityService.SaveAsync();

        String loginLink = $"https://app.example.com/login?email={adminUser.Email}";
        await _notificationClient.SendRegistrationEmailAsync(adminUser.Email, adminUser.FirstName, loginLink);

        return new IdResponseDto
        {
            Id = organization.Id,
            Message = "Organization registered successfully. Admin account created successfully."
        };
    }
}
