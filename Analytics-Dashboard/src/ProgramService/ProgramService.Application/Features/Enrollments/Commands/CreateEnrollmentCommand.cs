using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Domain.Entities;
using Shared.Authorization.Interfaces;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Identity;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Enrollments.Commands;

/// <summary>
/// Command to create a new enrollment for an employee in a program.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="EmployeeId">The ID of the employee to enroll.</param>
public record CreateEnrollmentCommand(Int64 ProgramId, Guid EmployeeId) : IRequest<EnrollmentCreatedDto>;

/// <summary>
/// Handles enrollment creation with employee validation, conflict checking, and notification.
/// </summary>
public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, EnrollmentCreatedDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityLookupClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly ILogger<CreateEnrollmentCommandHandler> _logger;
    private readonly IUserContextService _userContext;

    public CreateEnrollmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityLookupClient,
        INotificationGrpcClient notificationClient,
        ILogger<CreateEnrollmentCommandHandler> logger,
        IUserContextService userContext)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _identityLookupClient = identityLookupClient;
        _notificationClient = notificationClient;
        _logger = logger;
        _userContext = userContext;
    }

    /// <summary>
    /// Handles the enrollment creation process.
    /// </summary>
    /// <returns>The created enrollment data.</returns>
    public async Task<EnrollmentCreatedDto> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
        if (program is null)
            throw new NotFoundException("Program", request.ProgramId);

        EmployeeResponse? employee = await _identityLookupClient.GetEmployeeByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new BadRequestException("Employee does not exist.");
        if (!employee.IsActive)
            throw new BadRequestException("Employee is inactive.");
        if (employee.OrganizationId != program.OrganizationId)
            throw new BadRequestException("Employee does not belong to the program's organization.");

        Boolean alreadyEnrolled = await _programService.IsEmployeeEnrolledAsync(request.ProgramId, request.EmployeeId);
        if (alreadyEnrolled)
            throw new ConflictException("Employee is already enrolled in this program.");

        RefTermDto? enrollmentStatus = await _metaDataClient.GetByKeyAsync("EnrollmentStatus", "NotStarted", cancellationToken);

        Enrollment enrollment = new Enrollment
        {
            TrainingProgramId = request.ProgramId,
            EmployeeId = request.EmployeeId,
            CurrentWeek = 1,
            StatusId = enrollmentStatus is not null ? Guid.Parse(enrollmentStatus.Id) : null
        };

        await _programService.CreateEnrollmentAsync(enrollment);
        await _programService.SaveAsync();

        _logger.LogInformation("Employee {EmployeeId} enrolled in Program {ProgramId}", request.EmployeeId, request.ProgramId);

        await _notificationClient.SendInAppNotificationAsync(request.EmployeeId, "Enrolled in Program", $"You have been enrolled in program \"{program.Name}\".");

        return new EnrollmentCreatedDto
        {
            Id = enrollment.Id,
            EmployeeId = enrollment.EmployeeId
        };
    }
}
