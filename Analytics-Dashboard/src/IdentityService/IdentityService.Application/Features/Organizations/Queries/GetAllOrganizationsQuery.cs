using AutoMapper;
using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Organizations.Queries;

/// <summary>
/// Query to retrieve all organizations.
/// </summary>
public sealed record GetAllOrganizationsQuery() : IRequest<List<OrganizationResponseDto>>;

/// <summary>
/// Handles the <see cref="GetAllOrganizationsQuery"/> and returns a list of organization response DTOs.
/// </summary>
public sealed class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, List<OrganizationResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllOrganizationsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllOrganizationsQueryHandler"/> class.
    /// </summary>
    public GetAllOrganizationsQueryHandler(IIdentityService identityService, IMapper mapper, ILogger<GetAllOrganizationsQueryHandler> logger)
    {
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns all organizations.
    /// </summary>
    /// <returns>A list of organization response DTOs.</returns>
    public async Task<List<OrganizationResponseDto>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all organizations");
        List<Organization> organizations = await _identityService.GetAllOrganizationsAsync();
        return _mapper.Map<List<OrganizationResponseDto>>(organizations);
    }
}
