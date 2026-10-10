using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.Rbac;
using Shared.InterServiceHelper.Interfaces;

namespace Shared.InterServiceHelper.Services;

/// <summary>
/// Wraps the generated <see cref="RbacService.RbacServiceClient"/>.
/// This is the only class allowed to reference the generated gRPC client
/// directly; every other consumer depends on <see cref="IRbacGrpcClient"/>.
/// Responsible for building the protobuf request, attaching gRPC metadata,
/// enforcing a call deadline, translating gRPC exceptions into a
/// domain-friendly result, and mapping the protobuf response back to a
/// plain <see cref="bool"/>.
/// </summary>
public sealed class RbacGrpcClient : IRbacGrpcClient
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly RbacService.RbacServiceClient _client;
    private readonly ILogger<RbacGrpcClient> _logger;

    public RbacGrpcClient(RbacService.RbacServiceClient client, ILogger<RbacGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> CheckFeatureAccessAsync(
        Guid userId,
        string featureCode,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        Metadata headers = new Metadata();
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            headers.Add("authorization", bearerToken);
        }

        CheckFeatureAccessRequest request = new CheckFeatureAccessRequest
        {
            UserId = userId.ToString(),
            FeatureCode = featureCode
        };

        try
        {
            CheckFeatureAccessResponse response = await _client.CheckFeatureAccessAsync(
                request,
                headers,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            return response.IsAllowed;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(
                ex,
                "RBAC gRPC CheckFeatureAccess call failed for user {UserId} and feature {FeatureCode}. Status: {StatusCode}",
                userId,
                featureCode,
                ex.StatusCode);

            // Fail closed: a transport/service failure must never be
            // interpreted as "access granted".
            return false;
        }
    }
}
