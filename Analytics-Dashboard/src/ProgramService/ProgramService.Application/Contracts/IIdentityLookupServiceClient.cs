using Shared.InterServiceHelper.Grpc.Identity;

namespace ProgramService.Application.Contracts;

/// <summary>
/// gRPC client interface for looking up employee data from the Identity Service.
/// </summary>
public interface IIdentityLookupServiceClient
{
    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    /// <returns>The employee response if found; otherwise null.</returns>
    Task<EmployeeResponse?> GetEmployeeByIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all employees belonging to the specified organization.
    /// </summary>
    /// <returns>A list of employee items for the organization.</returns>
    Task<IReadOnlyList<EmployeeListItem>> GetEmployeesByOrganizationAsync(
        Int64 organizationId,
        CancellationToken cancellationToken = default);
}
