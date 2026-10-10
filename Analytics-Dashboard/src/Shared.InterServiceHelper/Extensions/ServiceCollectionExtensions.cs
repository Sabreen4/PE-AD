using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Grpc.Notification;
using Shared.InterServiceHelper.Grpc.Rbac;
using Shared.InterServiceHelper.Interfaces;
using Shared.InterServiceHelper.Services;

namespace Shared.InterServiceHelper.Extensions;

/// <summary>
/// Provides extension methods for registering interservice gRPC clients.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the gRPC client for MetaDataService's <see cref="MetaDataGrpcService"/>
    /// and <see cref="IMetaDataLookupServiceClient"/>, the wrapper other services should
    /// actually inject to resolve RefTerms for a RefSet they own.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application configuration containing service URLs.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the MetaData Service gRPC URL is not configured.
    /// </exception>
    public static IServiceCollection AddMetaDataGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        string metaDataServiceGrpcUrl = configuration["ServiceUrls:MetaDataServiceGrpc"]
            ?? throw new InvalidOperationException("Missing ServiceUrls:MetaDataServiceGrpc in configuration.");

        services.AddGrpcClient<MetaDataGrpcService.MetaDataGrpcServiceClient>(o =>
        {
            o.Address = new Uri(metaDataServiceGrpcUrl);
        });

        services.AddScoped<IMetaDataLookupServiceClient, MetaDataLookupServiceClient>();

        return services;
    }

    /// <summary>
    /// Registers the gRPC client for IdentityService's <see cref="RbacService"/>
    /// and <see cref="IRbacGrpcClient"/>, the wrapper
    /// <see cref="Shared.Authorization.Filters.FeatureAuthorizationFilter"/> depends on
    /// to check feature access for the current user.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application configuration containing service URLs.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Identity Service gRPC URL is not configured.
    /// </exception>
    public static IServiceCollection AddRbacGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        string identityServiceGrpcUrl = configuration["ServiceUrls:IdentityServiceGrpc"]
            ?? throw new InvalidOperationException("Missing ServiceUrls:IdentityServiceGrpc in configuration.");

        services.AddGrpcClient<RbacService.RbacServiceClient>(o =>
        {
            o.Address = new Uri(identityServiceGrpcUrl);
        });

        // IRbacGrpcClient wraps the generated gRPC client above; it is the
        // only class permitted to reference RbacService.RbacServiceClient.
        // FeatureAuthorizationFilter (via [FeatureAuthorize]) depends on it directly.
        services.AddScoped<IRbacGrpcClient, RbacGrpcClient>();

        return services;
    }

    /// <summary>
    /// Registers the gRPC client for NotificationService's <see cref="NotificationService.NotificationServiceClient"/>.
    /// </summary>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application configuration containing service URLs.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Notification Service gRPC URL is not configured.
    /// </exception>
    public static IServiceCollection AddNotificationGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        string notificationServiceGrpcUrl = configuration["ServiceUrls:NotificationServiceGrpc"]
            ?? throw new InvalidOperationException("Missing ServiceUrls:NotificationServiceGrpc in configuration.");

        services.AddGrpcClient<NotificationService.NotificationServiceClient>(o =>
        {
            o.Address = new Uri(notificationServiceGrpcUrl);
        });

        services.AddScoped<INotificationGrpcClient, NotificationGrpcClient>();

        return services;
    }
}
