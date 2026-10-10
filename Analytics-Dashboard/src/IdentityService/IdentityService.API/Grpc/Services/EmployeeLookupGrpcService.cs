using Grpc.Core;
using IdentityService.Application.Dtos.Users;
using IdentityService.Application.Features.Users.Queries;
using MediatR;
using Shared.InterServiceHelper.Grpc.Identity;

namespace IdentityService.API.Grpc.Services;

/// <summary>
/// gRPC service for looking up employee information including
/// individual employee details and organization-based employee listings.
/// </summary>
public sealed class EmployeeLookupGrpcService : IdentityLookupService.IdentityLookupServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeeLookupGrpcService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeLookupGrpcService"/> class.
    /// </summary>
    public EmployeeLookupGrpcService(IMediator mediator, ILogger<EmployeeLookupGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    /// <returns>An employee response indicating whether the employee exists along with their details.</returns>
    public override async Task<EmployeeResponse> GetEmployeeById(
        GetEmployeeByIdRequest request,
        ServerCallContext context)
    {
        try
        {
            Guid employeeId = Guid.Parse(request.EmployeeId);

            EmployeeResponseDto? employee = await _mediator.Send(
                new GetEmployeeByIdQuery(employeeId), context.CancellationToken);

            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                return new EmployeeResponse { Exists = false };
            }

            return new EmployeeResponse
            {
                Exists = true,
                Id = employee.Id.ToString(),
                FullName = employee.FullName,
                Email = employee.Email,
                IsActive = employee.IsActive,
                OrganizationId = employee.OrganizationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Employee lookup failed for {EmployeeId}", request.EmployeeId);
            return new EmployeeResponse { Exists = false };
        }
    }

    /// <summary>
    /// Retrieves all employees belonging to a specific organization.
    /// </summary>
    /// <returns>A response containing a list of employee items.</returns>
    public override async Task<GetEmployeesByOrganizationResponse> GetEmployeesByOrganization(
        GetEmployeesByOrganizationRequest request,
        ServerCallContext context)
    {
        try
        {
            List<EmployeeListItemDto> employees = await _mediator.Send(
                new GetEmployeesByOrganizationQuery(request.OrganizationId), context.CancellationToken);

            GetEmployeesByOrganizationResponse response = new GetEmployeesByOrganizationResponse();
            response.Employees.AddRange(employees.Select(e => new EmployeeListItem
            {
                Id = e.Id.ToString(),
                FullName = e.FullName,
                Email = e.Email,
                IsActive = e.IsActive,
                OrganizationId = e.OrganizationId
            }));

            _logger.LogInformation("Returned {Count} employees for Organization {OrgId}", employees.Count, request.OrganizationId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEmployeesByOrganization failed for Organization {OrgId}", request.OrganizationId);
            return new GetEmployeesByOrganizationResponse();
        }
    }
}
