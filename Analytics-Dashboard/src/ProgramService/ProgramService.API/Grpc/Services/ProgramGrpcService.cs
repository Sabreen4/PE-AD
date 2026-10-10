using Grpc.Core;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Identity;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;
using ProgramGrpc = Shared.InterServiceHelper.Grpc.Program;

namespace ProgramService.API.Grpc.Services;

/// <summary>
/// gRPC service exposing program operations including program details,
/// program weeks, assignments, enrollments, employee assignments,
/// and enrollment validation.
/// </summary>
public class ProgramGrpcService : ProgramGrpc.ProgramGrpcService.ProgramGrpcServiceBase
{
    private readonly IProgramService _programService;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityLookupClient;
    private readonly ILogger<ProgramGrpcService> _logger;

    public ProgramGrpcService(
        IProgramService programService,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityLookupClient,
        ILogger<ProgramGrpcService> logger)
    {
        _programService = programService;
        _metaDataClient = metaDataClient;
        _identityLookupClient = identityLookupClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves program details by ID, including the status key from metadata.
    /// </summary>
    /// <returns>A response containing program details or an indication that the program was not found.</returns>
    public override async Task<ProgramGrpc.GetProgramResponse> GetProgram(
        ProgramGrpc.GetProgramRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetProgram request: ProgramId={ProgramId}", request.ProgramId);

        try
        {
            TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
            if (program is null)
            {
                _logger.LogWarning("Program not found: {ProgramId}", request.ProgramId);
                return new ProgramGrpc.GetProgramResponse { Found = false };
            }

            String? statusKey = null;
            if (program.StatusId is not null)
            {
                RefTermDto? status = await _metaDataClient.GetByIdAsync(program.StatusId.Value, context.CancellationToken);
                if (status is not null)
                    statusKey = status.Key;
            }

            _logger.LogInformation("Program found: Id={ProgramId}, Name={Name}", request.ProgramId, program.Name);
            return new ProgramGrpc.GetProgramResponse
            {
                Found = true,
                ProgramId = request.ProgramId,
                OrganizationId = program.OrganizationId,
                EvaluatorId = program.EvaluatorId.ToString(),
                Name = program.Name,
                Description = program.Description,
                DurationWeeks = program.DurationWeeks,
                StatusId = statusKey ?? string.Empty
            };
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Program not found (NotFoundException): {ProgramId}", request.ProgramId);
            return new ProgramGrpc.GetProgramResponse { Found = false };
        }
    }

    /// <summary>
    /// Retrieves a program week by ID, optionally scoped to a program.
    /// </summary>
    /// <returns>A response containing program week details or an indication that the week was not found.</returns>
    public override async Task<ProgramGrpc.GetProgramWeekResponse> GetProgramWeek(
        ProgramGrpc.GetProgramWeekRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetProgramWeek request: ProgramWeekId={ProgramWeekId}, ProgramId={ProgramId}",
            request.ProgramWeekId, request.ProgramId);

        if (request.ProgramId > 0)
        {
            TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
            if (program is null)
            {
                _logger.LogWarning("Program not found for GetProgramWeek: ProgramId={ProgramId}", request.ProgramId);
                return new ProgramGrpc.GetProgramWeekResponse { Found = false };
            }
        }

        ProgramWeek? week = request.ProgramId > 0
            ? await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.ProgramWeekId)
            : await _programService.GetProgramWeekByIdAsync(request.ProgramWeekId);

        if (week is null)
        {
            _logger.LogWarning("ProgramWeek not found: {ProgramWeekId}", request.ProgramWeekId);
            return new ProgramGrpc.GetProgramWeekResponse { Found = false };
        }

        _logger.LogInformation("ProgramWeek found: Id={WeekId}, WeekNumber={WeekNumber}", week.Id, week.WeekNumber);
        return new ProgramGrpc.GetProgramWeekResponse
        {
            Found = true,
            Id = request.ProgramWeekId,
            ProgramId = week.TrainingProgramId,
            WeekNumber = week.WeekNumber,
            Title = week.Title,
            Description = week.Description
        };
    }

    /// <summary>
    /// Retrieves an assignment by ID, including the file type key from metadata.
    /// </summary>
    /// <returns>A response containing assignment details or an indication that the assignment was not found.</returns>
    public override async Task<ProgramGrpc.GetAssignmentResponse> GetAssignment(
        ProgramGrpc.GetAssignmentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetAssignment request: AssignmentId={AssignmentId}, ProgramId={ProgramId}, ProgramWeekId={ProgramWeekId}",
            request.AssignmentId, request.ProgramId, request.ProgramWeekId);

        try
        {
            Assignment? assignment = await _programService.GetAssignmentByProgramWeekAndIdAsync(
                request.ProgramId, request.ProgramWeekId, request.AssignmentId);
            if (assignment is null)
            {
                _logger.LogWarning("Assignment not found: {AssignmentId}", request.AssignmentId);
                return new ProgramGrpc.GetAssignmentResponse { Found = false };
            }

            String? fileTypeKey = null;
            if (assignment.FileTypeId.HasValue)
            {
                RefTermDto? fileType = await _metaDataClient.GetByIdAsync(assignment.FileTypeId.Value, context.CancellationToken);
                if (fileType is not null)
                    fileTypeKey = fileType.Key;
            }

            _logger.LogInformation("Assignment found: Id={AssignmentId}, Title={Title}", assignment.Id, assignment.Title);
            return new ProgramGrpc.GetAssignmentResponse
            {
                Found = true,
                Id = assignment.Id,
                ProgramId = assignment.TrainingProgramId,
                ProgramWeekId = assignment.ProgramWeekId ?? 0,
                FileTypeId = fileTypeKey ?? string.Empty,
                Title = assignment.Title,
                Description = assignment.Description,
                StartDate = assignment.StartDate.ToString("O"),
                EndDate = assignment.EndDate.ToString("O"),
                MaxMarks = assignment.MaxMarks
            };
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Assignment not found (NotFoundException): {AssignmentId}", request.AssignmentId);
            return new ProgramGrpc.GetAssignmentResponse { Found = false };
        }
    }

    /// <summary>
    /// Retrieves an enrollment by ID, including the status key from metadata.
    /// </summary>
    /// <returns>A response containing enrollment details or an indication that the enrollment was not found.</returns>
    public override async Task<ProgramGrpc.GetEnrollmentResponse> GetEnrollment(
        ProgramGrpc.GetEnrollmentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetEnrollment request: EnrollmentId={EnrollmentId}", request.EnrollmentId);

        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(request.EnrollmentId);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
            return new ProgramGrpc.GetEnrollmentResponse { Found = false };
        }

        String? statusKey = null;
        if (enrollment.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(enrollment.StatusId.Value, context.CancellationToken);
            if (status is not null)
                statusKey = status.Key;
        }

        _logger.LogInformation("Enrollment found: Id={EnrollmentId}, ProgramId={ProgramId}", enrollment.Id, enrollment.TrainingProgramId);
        return new ProgramGrpc.GetEnrollmentResponse
        {
            Found = true,
            Id = enrollment.Id,
            ProgramId = enrollment.TrainingProgramId,
            EmployeeId = enrollment.EmployeeId.ToString(),
            CurrentWeek = enrollment.CurrentWeek,
            StatusId = statusKey ?? string.Empty,
            IsActive = enrollment.IsActive
        };
    }

    /// <summary>
    /// Retrieves an employee assignment by ID, including related assignment, enrollment, and status information.
    /// </summary>
    /// <returns>A response containing employee assignment details or an indication that it was not found.</returns>
    public override async Task<ProgramGrpc.GetEmployeeAssignmentByIdResponse> GetEmployeeAssignmentById(
        ProgramGrpc.GetEmployeeAssignmentByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetEmployeeAssignmentById request: EmployeeAssignmentId={EmployeeAssignmentId}",
            request.EmployeeAssignmentId);

        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByIdAsync(
            request.EmployeeAssignmentId);

        if (employeeAssignment is null)
        {
            _logger.LogWarning("EmployeeAssignment not found: {EmployeeAssignmentId}", request.EmployeeAssignmentId);
            return new ProgramGrpc.GetEmployeeAssignmentByIdResponse { Found = false };
        }

        Assignment? assignment = await _programService.GetAssignmentByIdAsync(employeeAssignment.AssignmentId);
        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(employeeAssignment.EnrollmentId);

        String? statusKey = null;
        if (employeeAssignment.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(employeeAssignment.StatusId.Value, context.CancellationToken);
            if (status is not null)
                statusKey = status.Key;
        }

        _logger.LogInformation("EmployeeAssignment found: Id={EmployeeAssignmentId}, AssignmentId={AssignmentId}",
            employeeAssignment.Id, employeeAssignment.AssignmentId);

        ProgramGrpc.GetEmployeeAssignmentByIdResponse response = new ProgramGrpc.GetEmployeeAssignmentByIdResponse
        {
            Found = true,
            EmployeeAssignmentId = employeeAssignment.Id,
            AssignmentId = employeeAssignment.AssignmentId,
            EnrollmentId = employeeAssignment.EnrollmentId,
            EmployeeId = enrollment?.EmployeeId.ToString() ?? string.Empty,
            ProgramId = assignment?.TrainingProgramId ?? 0,
            ProgramWeekId = assignment?.ProgramWeekId ?? 0,
            MaxMarks = assignment?.MaxMarks ?? 0,
            ReviewerComment = employeeAssignment.ReviewerComment ?? string.Empty,
            StatusId = statusKey ?? string.Empty,
            IsActive = employeeAssignment.IsActive
        };

        if (employeeAssignment.Marks.HasValue)
            response.Marks = (double)employeeAssignment.Marks.Value;

        return response;
    }

    /// <summary>
    /// Validates whether an employee is currently enrolled in the specified program.
    /// </summary>
    /// <returns>A response indicating enrollment status and current week if enrolled.</returns>
    public override async Task<ProgramGrpc.ValidateEmployeeEnrollmentResponse> ValidateEmployeeEnrollment(
        ProgramGrpc.ValidateEmployeeEnrollmentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("ValidateEmployeeEnrollment request: ProgramId={ProgramId}, EmployeeId={EmployeeId}",
            request.ProgramId, request.EmployeeId);

        Guid employeeId = Guid.Parse(request.EmployeeId);
        Enrollment? enrollment = await _programService.GetActiveEnrollmentByEmployeeAsync(
            request.ProgramId, employeeId);

        if (enrollment is null)
        {
            _logger.LogWarning("Employee {EmployeeId} is not enrolled in program {ProgramId}", request.EmployeeId, request.ProgramId);
            return new ProgramGrpc.ValidateEmployeeEnrollmentResponse { IsEnrolled = false };
        }

        _logger.LogInformation("Employee {EmployeeId} is enrolled in program {ProgramId}", request.EmployeeId, request.ProgramId);
        return new ProgramGrpc.ValidateEmployeeEnrollmentResponse
        {
            IsEnrolled = true,
            EnrollmentId = enrollment.Id,
            CurrentWeek = enrollment.CurrentWeek
        };
    }
}
