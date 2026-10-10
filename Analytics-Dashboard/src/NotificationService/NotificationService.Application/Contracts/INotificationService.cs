using NotificationService.Domain.Entities;

namespace NotificationService.Application.Contracts;

/// <summary>
/// Defines operations for managing notifications and per-user notification read states.
/// </summary>
public interface INotificationService
{
    /// <summary>Creates a new notification entity.</summary>
    /// <param name="notification">The notification to create.</param>
    Task CreateNotificationAsync(Notification notification);

    /// <summary>Retrieves user notifications, optionally filtered by read state.</summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="isRead">Optional filter for read/unread state.</param>
    /// <returns>A list of user notifications.</returns>
    Task<List<UserNotification>> GetUserNotificationsAsync(Guid userId, bool? isRead);

    /// <summary>Gets a specific user notification by its ID.</summary>
    /// <param name="userNotificationId">The user notification ID.</param>
    /// <param name="userId">The user's ID for ownership validation.</param>
    /// <returns>The matching user notification, or null.</returns>
    Task<UserNotification?> GetUserNotificationByIdAsync(long userNotificationId, Guid userId);

    /// <summary>Creates a new user notification link.</summary>
    /// <param name="userNotification">The user notification to create.</param>
    Task CreateUserNotificationAsync(UserNotification userNotification);

    /// <summary>Updates an existing user notification.</summary>
    /// <param name="userNotification">The user notification with updated values.</param>
    void UpdateUserNotification(UserNotification userNotification);

    /// <summary>Deletes a user notification.</summary>
    /// <param name="userNotification">The user notification to delete.</param>
    void DeleteUserNotification(UserNotification userNotification);

    /// <summary>Retrieves all unread notifications for a given user.</summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A list of unread user notifications.</returns>
    Task<List<UserNotification>> GetUnreadNotificationsAsync(Guid userId);

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of affected rows.</returns>
    Task<int> SaveAsync();
}
