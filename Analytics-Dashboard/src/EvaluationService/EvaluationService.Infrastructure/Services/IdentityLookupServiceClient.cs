using EvaluationService.Application.Contracts;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.Identity;
using GeneratedIdentityLookupService = Shared.InterServiceHelper.Grpc.Identity.IdentityLookupService;

namespace EvaluationService.Infrastructure.Services;

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

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var request = new GetEmployeeByIdRequest { EmployeeId = employeeId.ToString() };

        try
        {
            var response = await _client.GetEmployeeByIdAsync(
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
                "gRPC call to IdentityLookupService.GetEmployeeById FAILED for {EmployeeId}. Status: {StatusCode}",
                employeeId, ex.StatusCode);
            throw;
        }
    }
}
