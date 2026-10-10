using Shared.Common.Entities;

namespace NotificationService.Domain.Entities
{
    /// <summary>
    /// Represents a notification that can be dispatched to one or more users.
    /// </summary>
    public sealed class Notification : BaseEntity
    {
        /// <summary>Gets or sets the unique identifier for the notification.</summary>
        public long Id { get; set; }

        /// <summary>Gets or sets the notification type identifier from the Metadata Service.</summary>
        public Guid NotificationTypeId { get; set; }

        /// <summary>Gets or sets the notification title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the notification message body.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets the collection of user notification links for this notification.</summary>
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}