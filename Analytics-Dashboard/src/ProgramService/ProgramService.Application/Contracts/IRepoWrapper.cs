using ProgramService.Application.Contracts.IRepository;

namespace ProgramService.Application.Contracts;

/// <summary>
/// Wraps all repository interfaces into a single unit of work for the Program Service.
/// </summary>
public interface IRepoWrapper
{
    /// <summary>Gets the program repository.</summary>
    IProgramRepository Program { get; }
    /// <summary>Gets the program week repository.</summary>
    IProgramWeekRepository ProgramWeek { get; }
    /// <summary>Gets the assignment repository.</summary>
    IAssignmentRepository Assignment { get; }
    /// <summary>Gets the enrollment repository.</summary>
    IEnrollmentRepository Enrollment { get; }
    /// <summary>Gets the employee assignment repository.</summary>
    IEmployeeAssignmentRepository EmployeeAssignment { get; }
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<Int32> SaveAsync();
}

