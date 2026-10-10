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

/// <summary>
/// Command to create a new employee assignment for a specific employee and assignment.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="ProgramWeekId">The ID of the program week.</param>
/// <param name="AssignmentId">The ID of the assignment.</param>
/// <param name="EmployeeId">The ID of the employee.</param>
public record CreateEmployeeAssignmentCommand(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, Guid EmployeeId) : IRequest<CreateEmployeeAssignmentResponseDto>;

/// <summary>
/// Handles the creation of an employee assignment with enrollment validation and conflict checking.
/// </summary>
public class CreateEmployeeAssignmentCommandHandler : IRequestHandler<CreateEmployeeAssignmentCommand, CreateEmployeeAssignmentResponseDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<CreateEmployeeAssignmentCommandHandler> _logger;

    public CreateEmployeeAssignmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<CreateEmployeeAssignmentCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the employee assignment creation process.
    /// </summary>
    /// <returns>The created employee assignment response.</returns>
    public async Task<CreateEmployeeAssignmentResponseDto> Handle(CreateEmployeeAssignmentCommand request, CancellationToken cancellationToken)
    {
        Assignment? assignment = await _programService.GetAssignmentByIdAsync(request.AssignmentId);
        if (assignment is null)
            throw new NotFoundException("Assignment", request.AssignmentId);

        Enrollment? enrollment = await _programService.GetActiveEnrollmentByEmployeeAsync(request.ProgramId, request.EmployeeId);
        if (enrollment is null)
            throw new BadRequestException("Employee is not enrolled in this program.");

        Boolean alreadyExists = await _programService.EmployeeAssignmentExistsAsync(request.AssignmentId, enrollment.Id);
        if (alreadyExists)
            throw new ConflictException("Employee already has an assignment for this item.");

        RefTermDto? notStartedStatus = await _metaDataClient.GetByKeyAsync("EmployeeAssignmentStatus", "NotStarted", cancellationToken);

        EmployeeAssignment employeeAssignment = new EmployeeAssignment
        {
            AssignmentId = request.AssignmentId,
            EnrollmentId = enrollment.Id,
            StatusId = notStartedStatus is not null ? Guid.Parse(notStartedStatus.Id) : null
        };

        await _programService.CreateEmployeeAssignmentAsync(employeeAssignment);
        await _programService.SaveAsync();

        _logger.LogInformation("EmployeeAssignment created for Assignment {AssignmentId}, Enrollment {EnrollmentId}", request.AssignmentId, enrollment.Id);

        return new CreateEmployeeAssignmentResponseDto
        {
            Id = employeeAssignment.Id,
            AssignmentId = employeeAssignment.AssignmentId,
            EnrollmentId = employeeAssignment.EnrollmentId,
            Status = notStartedStatus?.Key
        };
    }
}
