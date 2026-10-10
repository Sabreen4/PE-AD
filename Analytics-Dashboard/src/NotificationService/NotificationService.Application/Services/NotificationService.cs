using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Services;

/// <summary>
/// Application service managing notification and user notification entities
/// including creation, retrieval, and read/unread state management.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly IRepoWrapper _repo;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="repo">Unit-of-work wrapper for data access.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationService(IRepoWrapper repo, ILogger<NotificationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new notification entity in the database.
    /// </summary>
    /// <param name="notification">The notification to create.</param>
    public async Task CreateNotificationAsync(Notification notification)
    {
        _logger.LogInformation("Creating notification: Title={Title}", notification.Title);
        await _repo.Notification.CreateAsync(notification);
    }

    /// <summary>
    /// Retrieves user notifications, optionally filtered by read state.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="isRead">Optional filter for read/unread state.</param>
    /// <returns>A list of user notifications.</returns>
    public async Task<List<UserNotification>> GetUserNotificationsAsync(Guid userId, bool? isRead)
    {
        _logger.LogInformation("Getting notifications for user {UserId}, isRead={IsRead}", userId, isRead);
        IQueryable<UserNotification> query = _repo.UserNotification.FindByCondition(un => un.UserId == userId);
        if (isRead.HasValue)
            query = query.Where(un => un.IsRead == isRead.Value);
        return await query.Include(un => un.Notification).ToListAsync();
    }

    /// <summary>
    /// Gets a specific user notification by its ID, validating ownership.
    /// </summary>
    /// <param name="userNotificationId">The user notification ID.</param>
    /// <param name="userId">The user's ID for ownership validation.</param>
    /// <returns>The matching user notification, or null if not found.</returns>
    public async Task<UserNotification?> GetUserNotificationByIdAsync(long userNotificationId, Guid userId)
    {
        _logger.LogInformation("Getting user notification Id={UserNotificationId} for user {UserId}", userNotificationId, userId);
        return await _repo.UserNotification
            .FindByCondition(un => un.Id == userNotificationId && un.UserId == userId)
            .Include(un => un.Notification)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new user notification link associating a user with a notification.
    /// </summary>
    /// <param name="userNotification">The user notification to create.</param>
    public async Task CreateUserNotificationAsync(UserNotification userNotification)
    {
        _logger.LogInformation("Creating user notification for user {UserId}", userNotification.UserId);
        await _repo.UserNotification.CreateAsync(userNotification);
    }

    /// <summary>
    /// Updates an existing user notification (e.g. marking as read).
    /// </summary>
    /// <param name="userNotification">The user notification with updated values.</param>
    public void UpdateUserNotification(UserNotification userNotification)
    {
        _logger.LogInformation("Updating user notification Id={UserNotificationId}", userNotification.Id);
        _repo.UserNotification.Update(userNotification);
    }

    /// <summary>
    /// Deletes a user notification link.
    /// </summary>
    /// <param name="userNotification">The user notification to delete.</param>
    public void DeleteUserNotification(UserNotification userNotification)
    {
        _logger.LogInformation("Deleting user notification Id={UserNotificationId}", userNotification.Id);
        _repo.UserNotification.Delete(userNotification);
    }

    /// <summary>
    /// Retrieves all unread notifications for a given user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A list of unread user notifications.</returns>
    public async Task<List<UserNotification>> GetUnreadNotificationsAsync(Guid userId)
    {
        _logger.LogInformation("Getting unread notifications for user {UserId}", userId);
        return await _repo.UserNotification
            .FindByCondition(un => un.UserId == userId && !un.IsRead)
            .ToListAsync();
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving notification service changes");
        return await _repo.SaveAsync();
    }
}