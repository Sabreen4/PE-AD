using ProgramService.Domain.Entities;

namespace ProgramService.Application.Contracts;

/// <summary>
/// Application service interface for program domain operations including
/// training programs, program weeks, assignments, enrollments, and employee assignments.
/// </summary>
public interface IProgramService
{
    /// <summary>
    /// Retrieves all training programs.
    /// </summary>
    Task<List<TrainingPrograms>> GetAllProgramsAsync();
    /// <summary>
    /// Retrieves program IDs for which the specified employee has an active enrollment.
    /// </summary>
    Task<List<long>> GetEnrolledProgramIdsAsync(Guid employeeId);
    /// <summary>
    /// Retrieves a training program by its ID.
    /// </summary>
    Task<TrainingPrograms?> GetProgramByIdAsync(long id);
    /// <summary>
    /// Creates a new training program.
    /// </summary>
    Task CreateProgramAsync(TrainingPrograms program);
    /// <summary>
    /// Updates an existing training program.
    /// </summary>
    void UpdateProgram(TrainingPrograms program);

    // ProgramWeek
    /// <summary>
    /// Retrieves all program weeks for the specified program.
    /// </summary>
    Task<List<ProgramWeek>> GetProgramWeeksByProgramAsync(long programId);
    /// <summary>
    /// Retrieves a program week by its ID.
    /// </summary>
    Task<ProgramWeek?> GetProgramWeekByIdAsync(long weekId);
    /// <summary>
    /// Retrieves a program week by program ID and week ID.
    /// </summary>
    Task<ProgramWeek?> GetProgramWeekByProgramAndIdAsync(long programId, long weekId);
    /// <summary>
    /// Checks whether a program week with the given week number exists for the specified program.
    /// </summary>
    Task<bool> ProgramWeekExistsAsync(long programId, int weekNumber);
    /// <summary>
    /// Creates a new program week.
    /// </summary>
    Task CreateProgramWeekAsync(ProgramWeek week);
    /// <summary>
    /// Updates an existing program week.
    /// </summary>
    void UpdateProgramWeek(ProgramWeek week);

    // Assignment
    /// <summary>
    /// Retrieves all assignments for the specified program and program week.
    /// </summary>
    Task<List<Assignment>> GetAssignmentsByWeekAsync(long programId, long programWeekId);
    /// <summary>
    /// Retrieves an assignment by its ID.
    /// </summary>
    Task<Assignment?> GetAssignmentByIdAsync(long assignmentId);
    /// <summary>
    /// Retrieves an assignment scoped by program, program week, and assignment ID.
    /// </summary>
    Task<Assignment?> GetAssignmentByProgramWeekAndIdAsync(long programId, long programWeekId, long assignmentId);
    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    Task CreateAssignmentAsync(Assignment assignment);
    /// <summary>
    /// Updates an existing assignment.
    /// </summary>
    void UpdateAssignment(Assignment assignment);

    // Enrollment
    /// <summary>
    /// Retrieves all enrollments for the specified program.
    /// </summary>
    Task<List<Enrollment>> GetEnrollmentsByProgramAsync(long programId);
    /// <summary>
    /// Retrieves an enrollment by its ID.
    /// </summary>
    Task<Enrollment?> GetEnrollmentByIdAsync(long enrollmentId);
    /// <summary>
    /// Retrieves the active enrollment for a specific employee in the given program.
    /// </summary>
    Task<Enrollment?> GetActiveEnrollmentByEmployeeAsync(long programId, Guid employeeId);
    /// <summary>
    /// Checks whether an employee is enrolled in the given program.
    /// </summary>
    Task<bool> IsEmployeeEnrolledAsync(long programId, Guid employeeId);
    /// <summary>
    /// Creates a new enrollment.
    /// </summary>
    Task CreateEnrollmentAsync(Enrollment enrollment);
    /// <summary>
    /// Deletes an existing enrollment.
    /// </summary>
    void DeleteEnrollment(Enrollment enrollment);

    // EmployeeAssignment
    /// <summary>
    /// Retrieves all employee assignments for the specified assignment.
    /// </summary>
    Task<List<EmployeeAssignment>> GetEmployeeAssignmentsByAssignmentAsync(long assignmentId);
    /// <summary>
    /// Retrieves employee assignments for the specified assignment, optionally filtering by file name.
    /// </summary>
    Task<List<EmployeeAssignment>> GetEmployeeAssignmentsByAssignmentSearchAsync(long assignmentId, string? search);
    /// <summary>
    /// Retrieves an employee assignment by its ID.
    /// </summary>
    Task<EmployeeAssignment?> GetEmployeeAssignmentByIdAsync(long employeeAssignmentId);
    /// <summary>
    /// Retrieves an employee assignment scoped by assignment ID and employee assignment ID.
    /// </summary>
    Task<EmployeeAssignment?> GetEmployeeAssignmentByAssignmentAndIdAsync(long assignmentId, long employeeAssignmentId);
    /// <summary>
    /// Retrieves the employee assignment for the given assignment and enrollment.
    /// </summary>
    Task<EmployeeAssignment?> GetEmployeeAssignmentByAssignmentAndEnrollmentAsync(long assignmentId, long enrollmentId);
    /// <summary>
    /// Checks whether an employee assignment exists for the given assignment and enrollment.
    /// </summary>
    Task<bool> EmployeeAssignmentExistsAsync(long assignmentId, long enrollmentId);
    /// <summary>
    /// Creates a new employee assignment.
    /// </summary>
    Task CreateEmployeeAssignmentAsync(EmployeeAssignment employeeAssignment);
    /// <summary>
    /// Updates an existing employee assignment.
    /// </summary>
    void UpdateEmployeeAssignment(EmployeeAssignment employeeAssignment);

    // ProgramWeek video fields
    /// <summary>
    /// Updates the video fields of a program week.
    /// </summary>
    Task UpdateProgramWeekVideoAsync(ProgramWeek week);

    /// <summary>
    /// Retrieves multiple enrollments by their IDs.
    /// </summary>
    Task<List<Enrollment>> GetEnrollmentsByIdsAsync(List<long> enrollmentIds);
    /// <summary>
    /// Retrieves multiple training programs by their IDs.
    /// </summary>
    Task<List<TrainingPrograms>> GetProgramsByIdsAsync(List<long> programIds);
    /// <summary>
    /// Retrieves multiple program weeks by their IDs.
    /// </summary>
    Task<List<ProgramWeek>> GetProgramWeeksByIdsAsync(List<long> weekIds);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task<int> SaveAsync();

    /// <summary>
    /// Deletes a training program and all its child entities.
    /// </summary>
    Task DeleteProgramAsync(TrainingPrograms program);

    /// <summary>
    /// Deletes a program week and its child entities (assignments, employee assignments).
    /// </summary>
    Task DeleteProgramWeekAsync(ProgramWeek week);
}
