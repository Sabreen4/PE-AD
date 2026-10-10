using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.IRepository;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace NotificationService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing data operations on <see cref="UserNotification"/> entities.
/// </summary>
public sealed class UserNotificationRepository : RepositoryBase<UserNotification>, IUserNotificationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotificationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Logger instance.</param>
    public UserNotificationRepository(NotificationDbContext context, ILogger<UserNotificationRepository> logger) : base(context, logger)
    {
    }
}
