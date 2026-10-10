using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.IRepository;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace NotificationService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing data operations on <see cref="Notification"/> entities.
/// </summary>
public sealed class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Logger instance.</param>
    public NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger) : base(context, logger)
    {
    }
}
