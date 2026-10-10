using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Domain.Entities;
using ProgramService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace ProgramService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing database operations on ProgramWeek entities.
/// </summary>
public class ProgramWeekRepository : RepositoryBase<ProgramWeek>, IProgramWeekRepository
{
    public ProgramWeekRepository(ProgramDbContext context, ILogger<ProgramWeekRepository> logger) : base(context, logger) { }
}

