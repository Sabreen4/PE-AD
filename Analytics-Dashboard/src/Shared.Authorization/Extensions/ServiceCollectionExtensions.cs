using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authorization.Interfaces;
using Shared.Authorization.Services;
using Shared.InterServiceHelper.Grpc.Rbac;
using Shared.InterServiceHelper.Interfaces;
using Shared.InterServiceHelper.Services;

namespace Shared.Authorization.Extensions;

/// <summary>
/// Provides extension methods for registering shared authorization services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers shared authorization services, including the user context service,
    /// gRPC client for the Identity Service, and feature authorization service.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application configuration containing service URLs.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Identity Service gRPC URL is not configured.
    /// </exception>
    public static IServiceCollection AddSharedAuthorization(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string identityServiceGrpcUrl =
            configuration["ServiceUrls:IdentityServiceGrpc"]
            ?? throw new InvalidOperationException(
                "Missing ServiceUrls:IdentityServiceGrpc in configuration."
            );

        services.AddScoped<IUserContextService, UserContextService>();

        services.AddGrpcClient<RbacService.RbacServiceClient>(o =>
        {
            o.Address = new Uri(identityServiceGrpcUrl);
        });

        services.AddScoped<IRbacGrpcClient, RbacGrpcClient>();

        return services;
    }
}