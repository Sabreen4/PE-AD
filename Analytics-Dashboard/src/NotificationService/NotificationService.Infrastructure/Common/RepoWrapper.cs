using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Application.Contracts.IRepository;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using Shared.Authorization.Interfaces;

namespace NotificationService.Infrastructure.Common;

/// <summary>
/// Wraps all notification repository interfaces into a single unit of work
/// that coordinates persistence and audit fields via the shared DbContext.
/// </summary>
public sealed class RepoWrapper : IRepoWrapper
{
    private readonly NotificationDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly ILogger<RepoWrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private INotificationRepository? _notification;
    private IUserNotificationRepository? _userNotification;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepoWrapper"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="userContext">Service to retrieve the current user context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="loggerFactory">Factory for creating per-repository loggers.</param>
    public RepoWrapper(NotificationDbContext context, IUserContextService userContext, ILogger<RepoWrapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Gets the notification repository for CRUD operations on notifications.</summary>
    public INotificationRepository Notification
    {
        get { return _notification ??= new NotificationRepository(_context, _loggerFactory.CreateLogger<NotificationRepository>()); }
    }

    /// <summary>Gets the user notification repository for CRUD operations on user notification links.</summary>
    public IUserNotificationRepository UserNotification
    {
        get { return _userNotification ??= new UserNotificationRepository(_context, _loggerFactory.CreateLogger<UserNotificationRepository>()); }
    }

    private static readonly Guid SystemUserId = Guid.Parse("ffcb041f-f454-4499-b3e3-75bcea089b16");

    /// <summary>
    /// Persists all pending changes to the database, applying audit fields automatically.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<Int32> SaveAsync()
    {
        _logger.LogInformation("Saving changes to database");
        Guid currentUserId = _userContext.UserId ?? SystemUserId;
        int result = await _context.SaveAsync(currentUserId);
        _logger.LogInformation("Saved {Count} changes to database", result);
        return result;
    }
}
