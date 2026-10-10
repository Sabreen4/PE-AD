using AutoMapper;
using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace IdentityService.Application.Features.Organizations.Queries;

/// <summary>
/// Query to retrieve an organization by its unique identifier.
/// </summary>
public sealed record GetOrganizationQuery(Int64 Id) : IRequest<OrganizationResponseDto>;

/// <summary>
/// Handles the <see cref="GetOrganizationQuery"/> and returns the organization response DTO.
/// </summary>
public sealed class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrganizationResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizationQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrganizationQueryHandler"/> class.
    /// </summary>
    public GetOrganizationQueryHandler(IIdentityService identityService, IMapper mapper, ILogger<GetOrganizationQueryHandler> logger)
    {
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns the organization DTO.
    /// </summary>
    /// <returns>The organization response DTO.</returns>
    public async Task<OrganizationResponseDto> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting organization: {OrganizationId}", request.Id);

        Organization? organization = await _identityService.GetOrganizationByIdAsync(request.Id);
        if (organization is null)
        {
            _logger.LogWarning("Organization not found: {OrganizationId}", request.Id);
            throw new NotFoundException("Organization", request.Id);
        }

        return _mapper.Map<OrganizationResponseDto>(organization);
    }
}
