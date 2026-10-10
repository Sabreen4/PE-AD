using IdentityService.Application.Contracts;
using IdentityService.Application.Dtos.Users;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Users.Queries;

/// <summary>
/// Query to retrieve an employee by their unique identifier.
/// </summary>
public sealed record GetEmployeeByIdQuery(Guid EmployeeId) : IRequest<EmployeeResponseDto?>;

/// <summary>
/// Handles the <see cref="GetEmployeeByIdQuery"/> and returns the employee response DTO.
/// </summary>
public sealed class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponseDto?>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEmployeeByIdQueryHandler"/> class.
    /// </summary>
    public GetEmployeeByIdQueryHandler(IIdentityService identityService, ILogger<GetEmployeeByIdQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the query and returns the employee DTO if found and the user has the Employee role.
    /// </summary>
    /// <returns>The employee response DTO, or null if not found or not an employee.</returns>
    public async Task<EmployeeResponseDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee by id: {EmployeeId}", request.EmployeeId);

        User? user = await _identityService.GetUserByIdAsync(request.EmployeeId);
        if (user is null)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", request.EmployeeId);
            return null;
        }

        if (user.Role is null)
        {
            _logger.LogWarning("Employee {EmployeeId} has no role assigned", request.EmployeeId);
            return null;
        }

        bool isEmployee = user.Role.Name.Equals("Employee", StringComparison.OrdinalIgnoreCase);
        if (!isEmployee)
            return null;

        return new EmployeeResponseDto
        {
            Exists = true,
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            IsActive = user.IsActive,
            OrganizationId = user.OrganizationId
        };
    }
}
