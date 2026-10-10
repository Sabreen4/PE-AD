using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Users.Queries;

/// <summary>
/// Query to retrieve all employees belonging to a specific organization.
/// </summary>
public sealed record GetEmployeesByOrganizationQuery(long OrganizationId) : IRequest<List<EmployeeListItemDto>>;

/// <summary>
/// Handles the <see cref="GetEmployeesByOrganizationQuery"/> and returns a list of employee list item DTOs.
/// </summary>
public sealed class GetEmployeesByOrganizationQueryHandler : IRequestHandler<GetEmployeesByOrganizationQuery, List<EmployeeListItemDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetEmployeesByOrganizationQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEmployeesByOrganizationQueryHandler"/> class.
    /// </summary>
    public GetEmployeesByOrganizationQueryHandler(IIdentityService identityService, ILogger<GetEmployeesByOrganizationQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns the list of employees for the organization.
    /// </summary>
    /// <returns>A list of employee list item DTOs.</returns>
    public async Task<List<EmployeeListItemDto>> Handle(GetEmployeesByOrganizationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employees for OrganizationId: {OrganizationId}", request.OrganizationId);
        List<User> users = await _identityService.GetUsersByOrganizationAsync(request.OrganizationId);

        return users
            .Where(u => u.Role.Name.Equals("Employee", StringComparison.OrdinalIgnoreCase) && u.IsActive)
            .Select(u => new EmployeeListItemDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                IsActive = u.IsActive,
                OrganizationId = u.OrganizationId
            })
            .ToList();
    }
}
