using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Contracts.IRepository;
using ProgramService.Infrastructure.Persistence;
using ProgramService.Infrastructure.Repositories;
using Shared.Authorization.Interfaces;

namespace ProgramService.Infrastructure.Common;

/// <summary>
/// Unit of work implementation that wraps all repository instances and manages database context persistence.
/// </summary>
public class RepoWrapper : IRepoWrapper
{
    private readonly ProgramDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly ILogger<RepoWrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private IProgramRepository? _program;
    private IProgramWeekRepository? _programWeek;
    private IAssignmentRepository? _assignment;
    private IEnrollmentRepository? _enrollment;
    private IEmployeeAssignmentRepository? _employeeAssignment;

    public RepoWrapper(ProgramDbContext context, IUserContextService userContext, ILogger<RepoWrapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>Gets the program repository, lazily initialized.</summary>
    public IProgramRepository Program
    {
        get { return _program ??= new ProgramRepository(_context, _loggerFactory.CreateLogger<ProgramRepository>()); }
    }

    /// <summary>Gets the program week repository, lazily initialized.</summary>
    public IProgramWeekRepository ProgramWeek
    {
        get { return _programWeek ??= new ProgramWeekRepository(_context, _loggerFactory.CreateLogger<ProgramWeekRepository>()); }
    }

    /// <summary>Gets the assignment repository, lazily initialized.</summary>
    public IAssignmentRepository Assignment
    {
        get { return _assignment ??= new AssignmentRepository(_context, _loggerFactory.CreateLogger<AssignmentRepository>()); }
    }

    /// <summary>Gets the enrollment repository, lazily initialized.</summary>
    public IEnrollmentRepository Enrollment
    {
        get { return _enrollment ??= new EnrollmentRepository(_context, _loggerFactory.CreateLogger<EnrollmentRepository>()); }
    }

    /// <summary>Gets the employee assignment repository, lazily initialized.</summary>
    public IEmployeeAssignmentRepository EmployeeAssignment
    {
        get { return _employeeAssignment ??= new EmployeeAssignmentRepository(_context, _loggerFactory.CreateLogger<EmployeeAssignmentRepository>()); }
    }

    /// <summary>
    /// Persists all pending changes to the database with audit fields.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<Int32> SaveAsync()
    {
        _logger.LogInformation("Saving changes to database");
        Guid currentUserId = _userContext.UserId ?? Guid.Empty;
        int result = await _context.SaveAsync(currentUserId);
        _logger.LogInformation("Saved {Count} changes to database", result);
        return result;
    }
}

