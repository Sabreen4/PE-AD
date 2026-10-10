using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using Shared.InterServiceHelper.Grpc.Identity;
using GeneratedIdentityLookupService = Shared.InterServiceHelper.Grpc.Identity.IdentityLookupService;

namespace ProgramService.Infrastructure.Services;

/// <summary>
/// gRPC client for communicating with IdentityService to look up
/// employee details and organization-based employee listings.
/// </summary>
public sealed class IdentityLookupServiceClient : IIdentityLookupServiceClient
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly GeneratedIdentityLookupService.IdentityLookupServiceClient _client;
    private readonly ILogger<IdentityLookupServiceClient> _logger;

    public IdentityLookupServiceClient(
        GeneratedIdentityLookupService.IdentityLookupServiceClient client,
        ILogger<IdentityLookupServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        GetEmployeeByIdRequest request = new GetEmployeeByIdRequest
        {
            EmployeeId = employeeId.ToString()
        };

        try
        {
            EmployeeResponse response = await _client.GetEmployeeByIdAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            if (!response.Exists)
            {
                _logger.LogWarning("Employee {EmployeeId} not found in IdentityService", employeeId);
                return null;
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to IdentityLookupService.GetEmployeeById FAILED for {EmployeeId}. Status: {StatusCode} — IdentityService may be down or unreachable.",
                employeeId, ex.StatusCode);
            throw;
        }
    }

    public async Task<IReadOnlyList<EmployeeListItem>> GetEmployeesByOrganizationAsync(
        Int64 organizationId,
        CancellationToken cancellationToken = default)
    {
        GetEmployeesByOrganizationRequest request = new GetEmployeesByOrganizationRequest
        {
            OrganizationId = organizationId
        };

        try
        {
            GetEmployeesByOrganizationResponse response = await _client.GetEmployeesByOrganizationAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Retrieved {Count} employees for Organization {OrgId}",
                response.Employees.Count, organizationId);

            return response.Employees.ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex,
                "gRPC call to IdentityLookupService.GetEmployeesByOrganization failed for {OrgId}. Status: {StatusCode}",
                organizationId, ex.StatusCode);
            return Array.Empty<EmployeeListItem>();
        }
    }
}
