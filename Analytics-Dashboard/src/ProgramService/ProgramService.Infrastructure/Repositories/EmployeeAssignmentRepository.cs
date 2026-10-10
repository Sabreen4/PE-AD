using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Domain.Entities;
using ProgramService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace ProgramService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing database operations on EmployeeAssignment entities.
/// </summary>
public class EmployeeAssignmentRepository : RepositoryBase<EmployeeAssignment>, IEmployeeAssignmentRepository
{
    public EmployeeAssignmentRepository(ProgramDbContext context, ILogger<EmployeeAssignmentRepository> logger) : base(context, logger) { }
}

