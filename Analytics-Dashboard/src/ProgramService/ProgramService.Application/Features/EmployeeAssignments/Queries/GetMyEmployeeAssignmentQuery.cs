using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.EmployeeAssignments.Queries;

public record GetMyEmployeeAssignmentQuery(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, Guid UserId) : IRequest<EmployeeAssignmentEnrichedDto>;

public class GetMyEmployeeAssignmentQueryHandler : IRequestHandler<GetMyEmployeeAssignmentQuery, EmployeeAssignmentEnrichedDto>
{
    private readonly IProgramService _programService;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly ILogger<GetMyEmployeeAssignmentQueryHandler> _logger;

    public GetMyEmployeeAssignmentQueryHandler(
        IProgramService programService,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityClient,
        ILogger<GetMyEmployeeAssignmentQueryHandler> logger)
    {
        _programService = programService;
        _metaDataClient = metaDataClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<EmployeeAssignmentEnrichedDto> Handle(GetMyEmployeeAssignmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting my employee assignment for ProgramId: {ProgramId}, AssignmentId: {AssignmentId}, UserId: {UserId}",
            request.ProgramId, request.AssignmentId, request.UserId);

        Enrollment? enrollment = await _programService.GetActiveEnrollmentByEmployeeAsync(
            request.ProgramId, request.UserId);

        if (enrollment is null)
        {
            _logger.LogWarning("Employee {UserId} is not enrolled in program {ProgramId}", request.UserId, request.ProgramId);
            throw new NotFoundException("Enrollment", request.ProgramId);
        }

        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByAssignmentAndEnrollmentAsync(
            request.AssignmentId, enrollment.Id);

        if (employeeAssignment is null)
        {
            Assignment? assignment = await _programService.GetAssignmentByIdAsync(request.AssignmentId);
            if (assignment is null)
            {
                _logger.LogWarning("Assignment not found: {AssignmentId}", request.AssignmentId);
                throw new NotFoundException("Assignment", request.AssignmentId);
            }

            _logger.LogInformation("Auto-creating EmployeeAssignment for AssignmentId: {AssignmentId}, EnrollmentId: {EnrollmentId}",
                request.AssignmentId, enrollment.Id);

            RefTermDto? notStartedStatus = await _metaDataClient.GetByKeyAsync("EmployeeAssignmentStatus", "NotStarted", cancellationToken);

            employeeAssignment = new EmployeeAssignment
            {
                AssignmentId = request.AssignmentId,
                EnrollmentId = enrollment.Id,
                StatusId = notStartedStatus is not null ? Guid.Parse(notStartedStatus.Id) : null
            };

            await _programService.CreateEmployeeAssignmentAsync(employeeAssignment);
            await _programService.SaveAsync();

            _logger.LogInformation("Auto-created EmployeeAssignment {EmployeeAssignmentId}", employeeAssignment.Id);
        }

        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
        Assignment? fullAssignment = await _programService.GetAssignmentByIdAsync(employeeAssignment.AssignmentId);
        ProgramWeek? week = fullAssignment?.ProgramWeekId is not null
            ? await _programService.GetProgramWeekByIdAsync(fullAssignment.ProgramWeekId.Value)
            : null;

        (string Name, string Email) employee = (string.Empty, string.Empty);
        var employeeResponse = await _identityClient.GetEmployeeByIdAsync(enrollment.EmployeeId, cancellationToken);
        if (employeeResponse is not null)
            employee = (employeeResponse.FullName ?? string.Empty, employeeResponse.Email ?? string.Empty);

        var dto = new EmployeeAssignmentEnrichedDto
        {
            EmployeeAssignmentId = employeeAssignment.Id,
            EmployeeId = enrollment.EmployeeId,
            EmployeeName = employee.Name,
            EmployeeEmail = employee.Email,
            ProgramId = program?.Id ?? 0,
            ProgramName = program?.Name ?? string.Empty,
            ProgramWeekId = week?.Id ?? 0,
            ProgramWeekName = week is not null ? $"Week {week.WeekNumber} - {week.Title}" : string.Empty,
            AssignmentId = employeeAssignment.AssignmentId,
            AssignmentTitle = fullAssignment?.Title ?? string.Empty,
            EnrollmentId = employeeAssignment.EnrollmentId,
            Status = null,
            FileName = employeeAssignment.FileName,
            FileSize = employeeAssignment.FileSize,
            Marks = employeeAssignment.Marks,
            Comment = employeeAssignment.ReviewerComment,
            SubmittedAt = employeeAssignment.CreatedAt
        };

        if (employeeAssignment.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(employeeAssignment.StatusId.Value, cancellationToken);
            if (status is not null)
                dto.Status = status.Key;
        }

        _logger.LogInformation("Successfully retrieved employee assignment {EmployeeAssignmentId} for user {UserId}",
            dto.EmployeeAssignmentId, request.UserId);

        return dto;
    }
}
