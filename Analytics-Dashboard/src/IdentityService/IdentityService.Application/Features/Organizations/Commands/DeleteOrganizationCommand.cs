using IdentityService.Application.Contracts;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Organizations.Commands;

/// <summary>
/// Command to delete an organization by its unique identifier.
/// </summary>
public sealed record DeleteOrganizationCommand(Int64 Id) : IRequest<String>;

/// <summary>
/// Handles the <see cref="DeleteOrganizationCommand"/> by removing the organization from the database.
/// </summary>
public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, String>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<DeleteOrganizationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOrganizationCommandHandler"/> class.
    /// </summary>
    public DeleteOrganizationCommandHandler(IIdentityService identityService, ILogger<DeleteOrganizationCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the command and deletes the organization.
    /// </summary>
    /// <returns>A success message.</returns>
    public async Task<String> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        Int64 id = request.Id;

        _logger.LogInformation("Deleting organization: {OrganizationId}", id);

        Organization? organization = await _identityService.GetOrganizationByIdAsync(id);
        if (organization is null)
        {
            _logger.LogWarning("Organization not found: {OrganizationId}", id);
            throw new NotFoundException("Organization", id);
        }

        _identityService.DeleteOrganization(organization);
        await _identityService.SaveAsync();

        return "Organization deleted successfully";
    }
}
