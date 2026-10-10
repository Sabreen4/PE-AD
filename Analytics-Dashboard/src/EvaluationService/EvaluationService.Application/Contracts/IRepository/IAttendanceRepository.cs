using EvaluationService.Domain.Entities;
using Shared.Common.Interfaces;

namespace EvaluationService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for <see cref="Attendance"/> entities.
/// </summary>
public interface IAttendanceRepository : IRepositoryBase<Attendance>
{
}
