namespace Shared.InterServiceHelper.Interfaces;

/// <summary>
/// Abstraction over the generated RBAC gRPC client (<c>RbacService.RbacServiceClient</c>).
/// Exposes only the operations the application needs, in domain-friendly terms,
/// and hides all protobuf/gRPC types from callers.
/// </summary>
public interface IRbacGrpcClient
{
    /// <summary>
    /// Checks whether the given user has access to the given feature by calling
    /// the Identity Service's RBAC gRPC endpoint.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="featureCode">The unique code of the feature being checked.</param>
    /// <param name="bearerToken">
    /// Optional bearer token forwarded to the Identity Service as an
    /// <c>authorization</c> gRPC metadata header.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// <see langword="true"/> if the user is allowed to use the feature;
    /// otherwise <see langword="false"/> (including when the call itself fails).
    /// </returns>
    Task<bool> CheckFeatureAccessAsync(
        Guid userId,
        string featureCode,
        string? bearerToken = null,
        CancellationToken cancellationToken = default);
}
