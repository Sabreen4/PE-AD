using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Domain.Entities;
using ProgramService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace ProgramService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing database operations on Assignment entities.
/// </summary>
public class AssignmentRepository : RepositoryBase<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(ProgramDbContext context, ILogger<AssignmentRepository> logger) : base(context, logger) { }
}

