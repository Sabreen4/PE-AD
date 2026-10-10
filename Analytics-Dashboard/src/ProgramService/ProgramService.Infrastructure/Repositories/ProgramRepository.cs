using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Domain.Entities;
using ProgramService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace ProgramService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing database operations on TrainingPrograms entities.
/// </summary>
public class ProgramRepository : RepositoryBase<TrainingPrograms>, IProgramRepository
{
    public ProgramRepository(ProgramDbContext context, ILogger<ProgramRepository> logger) : base(context, logger) { }
}

