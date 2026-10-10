using EvaluationService.Application.Contracts.IRepository;
using EvaluationService.Domain.Entities;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="Attendance"/> entities.
/// </summary>
public sealed class AttendanceRepository : RepositoryBase<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(EvaluationDbContext context, ILogger<AttendanceRepository> logger)
        : base(context, logger)
    {
    }
}
