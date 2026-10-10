using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.EmployeeAssignments.Queries;

public record GetEmployeeAssignmentQuery(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, Int64 EmployeeAssignmentId) : IRequest<EmployeeAssignmentEnrichedDto>;

public class GetEmployeeAssignmentQueryHandler : IRequestHandler<GetEmployeeAssignmentQuery, EmployeeAssignmentEnrichedDto>
{
    private readonly IProgramService _programService;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly ILogger<GetEmployeeAssignmentQueryHandler> _logger;

    public GetEmployeeAssignmentQueryHandler(
        IProgramService programService,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityClient,
        ILogger<GetEmployeeAssignmentQueryHandler> logger)
    {
        _programService = programService;
        _metaDataClient = metaDataClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<EmployeeAssignmentEnrichedDto> Handle(GetEmployeeAssignmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee assignment: {EmployeeAssignmentId} for AssignmentId: {AssignmentId}", request.EmployeeAssignmentId, request.AssignmentId);

        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByAssignmentAndIdAsync(request.AssignmentId, request.EmployeeAssignmentId);
        if (employeeAssignment is null)
        {
            _logger.LogWarning("EmployeeAssignment not found: {EmployeeAssignmentId}", request.EmployeeAssignmentId);
            throw new NotFoundException("EmployeeAssignment", request.EmployeeAssignmentId);
        }

        Enrollment? enrollment = employeeAssignment.EnrollmentId > 0
            ? await _programService.GetEnrollmentByIdAsync(employeeAssignment.EnrollmentId)
            : null;
        TrainingPrograms? program = enrollment is not null
            ? await _programService.GetProgramByIdAsync(enrollment.TrainingProgramId)
            : null;
        Assignment? assignment = await _programService.GetAssignmentByIdAsync(employeeAssignment.AssignmentId);
        ProgramWeek? week = assignment?.ProgramWeekId is not null
            ? await _programService.GetProgramWeekByIdAsync(assignment.ProgramWeekId.Value)
            : null;

        (string Name, string Email) employee = (string.Empty, string.Empty);
        if (enrollment is not null)
        {
            var employeeResponse = await _identityClient.GetEmployeeByIdAsync(enrollment.EmployeeId, cancellationToken);
            if (employeeResponse is not null)
                employee = (employeeResponse.FullName ?? string.Empty, employeeResponse.Email ?? string.Empty);
        }

        var dto = new EmployeeAssignmentEnrichedDto
        {
            EmployeeAssignmentId = employeeAssignment.Id,
            EmployeeId = enrollment?.EmployeeId ?? Guid.Empty,
            EmployeeName = employee.Name,
            EmployeeEmail = employee.Email,
            ProgramId = program?.Id ?? 0,
            ProgramName = program?.Name ?? string.Empty,
            ProgramWeekId = week?.Id ?? 0,
            ProgramWeekName = week is not null ? $"Week {week.WeekNumber} - {week.Title}" : string.Empty,
            AssignmentId = employeeAssignment.AssignmentId,
            AssignmentTitle = assignment?.Title ?? string.Empty,
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

        return dto;
    }
}
