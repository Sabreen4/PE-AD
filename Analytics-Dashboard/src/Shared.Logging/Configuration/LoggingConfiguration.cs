using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Shared.Logging.Configuration;

/// <summary>
/// Provides extension methods for configuring shared Serilog logging
/// across all microservices.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with console and rolling file sinks enriched with
    /// machine name, thread ID, and service name.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure logging on.</param>
    /// <param name="serviceName">The name of the service for log enrichment and file naming.</param>
    /// <returns>The host builder with logging configured.</returns>
    public static IHostBuilder AddSharedLogging(
        this IHostBuilder hostBuilder,
        string serviceName)
    {
        hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console()
                .WriteTo.File(
                    $"logs/{serviceName}-.log",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Warning);
        });

        return hostBuilder;
    }

    /// <summary>
    /// Configures Serilog logging on a <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The web application builder to configure logging on.</param>
    /// <param name="serviceName">The name of the service for log enrichment and file naming.</param>
    /// <returns>The web application builder with logging configured.</returns>
    public static WebApplicationBuilder AddSharedLogging(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Host.AddSharedLogging(serviceName);
        return builder;
    }

    /// <summary>
    /// Enables Serilog request logging for the web application.
    /// </summary>
    /// <param name="app">The web application to add request logging to.</param>
    /// <returns>The web application with request logging enabled.</returns>
    public static WebApplication UseSharedRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}