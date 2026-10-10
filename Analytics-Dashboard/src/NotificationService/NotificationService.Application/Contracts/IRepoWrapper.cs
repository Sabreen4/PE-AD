using NotificationService.Application.Contracts.IRepository;

namespace NotificationService.Application.Contracts;

/// <summary>
/// Unit-of-work interface that groups all notification repositories and coordinates database persistence.
/// </summary>
public interface IRepoWrapper
{
    /// <summary>Gets the notification repository for CRUD operations on notifications.</summary>
    INotificationRepository Notification { get; }

    /// <summary>Gets the user notification repository for CRUD operations on user notification links.</summary>
    IUserNotificationRepository UserNotification { get; }

    /// <summary>Persists all pending changes to the database, applying audit fields automatically.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<Int32> SaveAsync();
}