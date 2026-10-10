using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Background service that periodically cleans up expired and verified OTP records from the database.
/// </summary>
public sealed class ExpiredOtpCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredOtpCleanupService> _logger;

    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiredOtpCleanupService"/> class.
    /// </summary>
    public ExpiredOtpCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredOtpCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the cleanup loop at a fixed interval until the application stops.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired OTP cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredOtpsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up expired OTPs");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Deletes all expired or verified OTP records from the database.
    /// </summary>
    private async Task CleanupExpiredOtpsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        int deleted = await context.VerifyOtps
            .Where(o => o.ExpiryDate <= DateTime.UtcNow || o.IsVerified)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Cleaned up {Count} expired/verified OTP records", deleted);
    }
}
