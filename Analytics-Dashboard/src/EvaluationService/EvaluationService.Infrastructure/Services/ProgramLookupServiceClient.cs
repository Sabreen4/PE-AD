using EvaluationService.Application.Contracts;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Program;
using GeneratedProgramGrpcService = Shared.InterServiceHelper.Grpc.Program.ProgramGrpcService;

namespace EvaluationService.Infrastructure.Services;

/// <summary>
/// Client for communicating with the Program gRPC service to look up enrollments, program weeks, programs, and employee assignments.
/// </summary>
public sealed class ProgramLookupServiceClient : IProgramLookupServiceClient
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(5);

    private readonly GeneratedProgramGrpcService.ProgramGrpcServiceClient _client;
    private readonly ILogger<ProgramLookupServiceClient> _logger;

    public ProgramLookupServiceClient(
        GeneratedProgramGrpcService.ProgramGrpcServiceClient client,
        ILogger<ProgramLookupServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves enrollment details by enrollment ID via gRPC.
    /// </summary>
    /// <param name="enrollmentId">The enrollment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enrollment response if found; otherwise null.</returns>
    public async Task<GetEnrollmentResponse?> GetEnrollmentByIdAsync(
        long enrollmentId, CancellationToken cancellationToken = default)
    {
        GetEnrollmentRequest request = new GetEnrollmentRequest
        {
            EnrollmentId = enrollmentId
        };

        try
        {
            GetEnrollmentResponse response = await _client.GetEnrollmentAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("Enrollment {EnrollmentId} not found in ProgramService", enrollmentId);
                return null;
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to ProgramService.GetEnrollment FAILED for {EnrollmentId}. Status: {StatusCode}",
                enrollmentId, ex.StatusCode);

            if (ex.StatusCode == StatusCode.Unavailable)
                throw new ServiceUnavailableException("ProgramService is currently unavailable.");

            return null;
        }
    }

    /// <summary>
    /// Retrieves program week details by program week ID via gRPC.
    /// </summary>
    /// <param name="programWeekId">The program week identifier.</param>
    /// <param name="programId">Optional program identifier for validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The program week response if found; otherwise null.</returns>
    public async Task<GetProgramWeekResponse?> GetProgramWeekByIdAsync(
        long programWeekId, long programId = 0, CancellationToken cancellationToken = default)
    {
        GetProgramWeekRequest request = new GetProgramWeekRequest
        {
            ProgramWeekId = programWeekId,
            ProgramId = programId
        };

        try
        {
            GetProgramWeekResponse response = await _client.GetProgramWeekAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("ProgramWeek {ProgramWeekId} not found in ProgramService", programWeekId);
                return null;
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to ProgramService.GetProgramWeek FAILED for {ProgramWeekId}. Status: {StatusCode}",
                programWeekId, ex.StatusCode);

            if (ex.StatusCode == StatusCode.Unavailable)
                throw new ServiceUnavailableException("ProgramService is currently unavailable.");

            return null;
        }
    }

    /// <summary>
    /// Retrieves program details by program ID via gRPC.
    /// </summary>
    /// <param name="programId">The program identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The program response if found; otherwise null.</returns>
    public async Task<GetProgramResponse?> GetProgramByIdAsync(
        long programId, CancellationToken cancellationToken = default)
    {
        GetProgramRequest request = new GetProgramRequest
        {
            ProgramId = programId
        };

        try
        {
            GetProgramResponse response = await _client.GetProgramAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("Program {ProgramId} not found in ProgramService", programId);
                return null;
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to ProgramService.GetProgram FAILED for {ProgramId}. Status: {StatusCode}",
                programId, ex.StatusCode);

            if (ex.StatusCode == StatusCode.Unavailable)
                throw new ServiceUnavailableException("ProgramService is currently unavailable.");

            return null;
        }
    }

    /// <summary>
    /// Retrieves employee assignment details by assignment ID via gRPC.
    /// </summary>
    /// <param name="employeeAssignmentId">The employee assignment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The employee assignment response if found; otherwise null.</returns>
    public async Task<GetEmployeeAssignmentByIdResponse?> GetEmployeeAssignmentByIdAsync(
        long employeeAssignmentId, CancellationToken cancellationToken = default)
    {
        GetEmployeeAssignmentByIdRequest request = new GetEmployeeAssignmentByIdRequest
        {
            EmployeeAssignmentId = employeeAssignmentId
        };

        try
        {
            GetEmployeeAssignmentByIdResponse response = await _client.GetEmployeeAssignmentByIdAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("EmployeeAssignment {EmployeeAssignmentId} not found in ProgramService", employeeAssignmentId);
                return null;
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to ProgramService.GetEmployeeAssignmentById FAILED for {EmployeeAssignmentId}. Status: {StatusCode}",
                employeeAssignmentId, ex.StatusCode);

            if (ex.StatusCode == StatusCode.Unavailable)
                throw new ServiceUnavailableException("ProgramService is currently unavailable.");

            return null;
        }
    }

    /// <summary>
    /// Validates whether an employee is enrolled in a specific program via gRPC.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="programId">The program identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation response.</returns>
    public async Task<ValidateEmployeeEnrollmentResponse?> ValidateEmployeeEnrollmentAsync(
        string employeeId, long programId, CancellationToken cancellationToken = default)
    {
        ValidateEmployeeEnrollmentRequest request = new ValidateEmployeeEnrollmentRequest
        {
            EmployeeId = employeeId,
            ProgramId = programId
        };

        try
        {
            ValidateEmployeeEnrollmentResponse response = await _client.ValidateEmployeeEnrollmentAsync(
                request,
                deadline: DateTime.UtcNow.Add(CallTimeout),
                cancellationToken: cancellationToken);

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC call to ProgramService.ValidateEmployeeEnrollment FAILED for employee {EmployeeId}, program {ProgramId}. Status: {StatusCode}",
                employeeId, programId, ex.StatusCode);

            if (ex.StatusCode == StatusCode.Unavailable)
                throw new ServiceUnavailableException("ProgramService is currently unavailable.");

            return null;
        }
    }
}
