using Shared.InterServiceHelper.Grpc.Program;

namespace EvaluationService.Application.Contracts;

/// <summary>
/// Abstraction for communicating with the Program gRPC service to retrieve enrollment, program week, program, and employee assignment data.
/// </summary>
public interface IProgramLookupServiceClient
{
    /// <summary>
    /// Retrieves enrollment details by enrollment ID.
    /// </summary>
    Task<GetEnrollmentResponse?> GetEnrollmentByIdAsync(long enrollmentId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves program week details by program week ID.
    /// </summary>
    Task<GetProgramWeekResponse?> GetProgramWeekByIdAsync(long programWeekId, long programId = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves program details by program ID.
    /// </summary>
    Task<GetProgramResponse?> GetProgramByIdAsync(long programId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves employee assignment details by assignment ID.
    /// </summary>
    Task<GetEmployeeAssignmentByIdResponse?> GetEmployeeAssignmentByIdAsync(long employeeAssignmentId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Validates whether an employee is enrolled in a specific program.
    /// </summary>
    Task<ValidateEmployeeEnrollmentResponse?> ValidateEmployeeEnrollmentAsync(string employeeId, long programId, CancellationToken cancellationToken = default);
}
