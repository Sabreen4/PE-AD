using NotificationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace NotificationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for performing data operations on <see cref="Notification"/> entities.
/// </summary>
public interface INotificationRepository : IRepositoryBase<Notification>
{
}
