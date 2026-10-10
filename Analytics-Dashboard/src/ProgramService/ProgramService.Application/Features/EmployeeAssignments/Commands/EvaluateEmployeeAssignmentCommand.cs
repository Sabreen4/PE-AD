using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.EmployeeAssignments.Commands;

public record EvaluateEmployeeAssignmentCommand(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, Int64 EmployeeAssignmentId, EvaluateEmployeeAssignmentDto Dto) : IRequest<EmployeeAssignmentEnrichedDto>;

public class EvaluateEmployeeAssignmentCommandHandler
    : IRequestHandler<EvaluateEmployeeAssignmentCommand, EmployeeAssignmentEnrichedDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly ILogger<EvaluateEmployeeAssignmentCommandHandler> _logger;

    public EvaluateEmployeeAssignmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        IIdentityLookupServiceClient identityClient,
        ILogger<EvaluateEmployeeAssignmentCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<EmployeeAssignmentEnrichedDto> Handle(EvaluateEmployeeAssignmentCommand request, CancellationToken cancellationToken)
    {
        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByAssignmentAndIdAsync(request.AssignmentId, request.EmployeeAssignmentId);
        if (employeeAssignment is null)
            throw new NotFoundException("EmployeeAssignment", request.EmployeeAssignmentId);

        RefTermDto? evaluatedStatus = await _metaDataClient.GetByKeyAsync("EmployeeAssignmentStatus", "Evaluated", cancellationToken);

        employeeAssignment.Marks = request.Dto.Marks;
        employeeAssignment.ReviewerComment = request.Dto.ReviewerComment;
        employeeAssignment.StatusId = evaluatedStatus is not null ? Guid.Parse(evaluatedStatus.Id) : null;

        _programService.UpdateEmployeeAssignment(employeeAssignment);
        await _programService.SaveAsync();

        _logger.LogInformation("EmployeeAssignment {Id} evaluated with marks {Marks}", request.EmployeeAssignmentId, request.Dto.Marks);

        Assignment? assignment = await _programService.GetAssignmentByIdAsync(request.AssignmentId);
        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(employeeAssignment.EnrollmentId);
        if (enrollment is not null && assignment is not null)
        {
            await _notificationClient.SendInAppNotificationAsync(
                enrollment.EmployeeId,
                "Assignment Evaluated",
                $"Your assignment \"{assignment.Title}\" has been evaluated with {request.Dto.Marks} marks.");
        }

        TrainingPrograms? program = enrollment is not null
            ? await _programService.GetProgramByIdAsync(enrollment.TrainingProgramId)
            : null;
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
