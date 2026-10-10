using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Organizations.Commands;

/// <summary>
/// Command to update an existing organization's details.
/// </summary>
public sealed record UpdateOrganizationCommand(Int64 Id, CreateOrganizationRequestDto Dto) : IRequest<IdResponseDto>;

/// <summary>
/// Handles the <see cref="UpdateOrganizationCommand"/> by updating the organization in the database.
/// </summary>
public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, IdResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<UpdateOrganizationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateOrganizationCommandHandler"/> class.
    /// </summary>
    public UpdateOrganizationCommandHandler(IIdentityService identityService, ILogger<UpdateOrganizationCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command and updates the organization.
    /// </summary>
    /// <returns>An ID response DTO with the organization ID.</returns>
    public async Task<IdResponseDto> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        Int64 id = request.Id;
        CreateOrganizationRequestDto dto = request.Dto;

        _logger.LogInformation("Updating organization: {OrganizationId}", id);

        Organization? organization = await _identityService.GetOrganizationByIdAsync(id);
        if (organization is null)
        {
            _logger.LogWarning("Organization not found: {OrganizationId}", id);
            throw new NotFoundException("Organization", id);
        }

        organization.Name = dto.OrganizationName;
        organization.Email = dto.OrganizationEmail;
        organization.Phone = dto.OrganizationPhone;
        organization.City = dto.City;

        _identityService.UpdateOrganization(organization);
        await _identityService.SaveAsync();

        return new IdResponseDto
        {
            Id = id,
            Message = "Organization updated successfully"
        };
    }
}
