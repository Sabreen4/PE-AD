using Shared.Common.Entities;

namespace NotificationService.Domain.Entities
{
    /// <summary>
    /// Represents the delivery/read status of a notification for a specific user.
    /// </summary>
    public sealed class UserNotification : BaseEntity
    {
        /// <summary>Gets or sets the unique identifier for this user notification record.</summary>
        public long Id { get; set; }

        /// <summary>Gets or sets the ID of the user this notification is targeted to.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets the notification ID (same-service foreign key).</summary>
        public long NotificationId { get; set; }

        /// <summary>Gets or sets a value indicating whether the notification has been read by the user.</summary>
        public bool IsRead { get; set; }

        /// <summary>Gets or sets the timestamp when the notification was read, if applicable.</summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>Gets or sets the associated notification entity.</summary>
        public Notification Notification { get; set; } = null!;
    }
}