using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Assignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Assignments.Commands;

/// <summary>
/// Command to create a new assignment for a program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="ProgramWeekId">The ID of the program week.</param>
/// <param name="Dto">The assignment creation data.</param>
public record CreateAssignmentCommand(Int64 ProgramId, Int64 ProgramWeekId, CreateAssignmentDto Dto) : IRequest<AssignmentCreatedDto>;

/// <summary>
/// Handles the creation of an assignment, including file type resolution and employee notifications.
/// </summary>
public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, AssignmentCreatedDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly ILogger<CreateAssignmentCommandHandler> _logger;

    public CreateAssignmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        ILogger<CreateAssignmentCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the assignment creation process.
    /// </summary>
    /// <returns>The created assignment data.</returns>
    public async Task<AssignmentCreatedDto> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        ProgramWeek? week = await _programService.GetProgramWeekByProgramAndIdAsync(request.ProgramId, request.ProgramWeekId);
        if (week is null)
            throw new NotFoundException("ProgramWeek", request.ProgramWeekId);

        Assignment assignment = new Assignment
        {
            TrainingProgramId = request.ProgramId,
            ProgramWeekId = request.ProgramWeekId,
            FileTypeId = null,
            FilePath = request.Dto.FilePath ?? string.Empty,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            StartDate = request.Dto.StartDate,
            EndDate = request.Dto.EndDate,
            MaxMarks = request.Dto.MaxMarks
        };

        if (!string.IsNullOrEmpty(request.Dto.FileType))
        {
            RefTermDto? fileTypeRefTerm = await _metaDataClient.GetByKeyAsync("FileType", request.Dto.FileType, cancellationToken);
            if (fileTypeRefTerm is null)
                throw new BadRequestException($"Invalid FileType '{request.Dto.FileType}'.");
            assignment.FileTypeId = Guid.Parse(fileTypeRefTerm.Id);
        }

        await _programService.CreateAssignmentAsync(assignment);
        await _programService.SaveAsync();

        List<Enrollment> enrollments = await _programService.GetEnrollmentsByProgramAsync(request.ProgramId);
        foreach (Enrollment enrollment in enrollments.Where(e => e.IsActive))
        {
            await _notificationClient.SendInAppNotificationAsync(enrollment.EmployeeId, "New Assignment", $"New assignment \"{assignment.Title}\" has been created.");
        }

        return new AssignmentCreatedDto
        {
            Id = assignment.Id,
            Title = assignment.Title
        };
    }
}
