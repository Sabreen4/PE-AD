using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Domain.Entities;
using ProgramService.Infrastructure.Persistence;
using Shared.Common.Repositories;

namespace ProgramService.Infrastructure.Repositories;

/// <summary>
/// Repository for performing database operations on Enrollment entities.
/// </summary>
public class EnrollmentRepository : RepositoryBase<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ProgramDbContext context, ILogger<EnrollmentRepository> logger) : base(context, logger) { }
}

