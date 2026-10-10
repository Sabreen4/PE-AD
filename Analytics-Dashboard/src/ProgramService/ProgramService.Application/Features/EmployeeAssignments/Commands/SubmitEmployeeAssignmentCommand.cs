using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.EmployeeAssignments.Commands;

public record SubmitEmployeeAssignmentCommand(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, Int64 EmployeeAssignmentId, Stream FileContent, String FileName, Int64 FileSize) : IRequest<SubmitEmployeeAssignmentResponseDto>;

public class SubmitEmployeeAssignmentCommandHandler : IRequestHandler<SubmitEmployeeAssignmentCommand, SubmitEmployeeAssignmentResponseDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly FileStorageOptions _storageOptions;
    private readonly ILogger<SubmitEmployeeAssignmentCommandHandler> _logger;

    public SubmitEmployeeAssignmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        IOptions<FileStorageOptions> storageOptions,
        ILogger<SubmitEmployeeAssignmentCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task<SubmitEmployeeAssignmentResponseDto> Handle(SubmitEmployeeAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (request.FileContent is null || request.FileContent.Length == 0)
            throw new BadRequestException("File is required.");

        long maxBytes = _storageOptions.MaxFileSizeMB * 1024L * 1024L;
        if (request.FileSize > maxBytes)
            throw new BadRequestException($"File size exceeds the maximum allowed size of {_storageOptions.MaxFileSizeMB} MB.");

        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByAssignmentAndIdAsync(request.AssignmentId, request.EmployeeAssignmentId);
        if (employeeAssignment is null)
            throw new NotFoundException("EmployeeAssignment", request.EmployeeAssignmentId);

        Assignment? assignment = await _programService.GetAssignmentByIdAsync(request.AssignmentId);
        if (assignment is null)
            throw new NotFoundException("Assignment", request.AssignmentId);

        if (assignment.EndDate < DateTime.UtcNow)
            throw new BadRequestException("Assignment submission deadline has passed.");

        if (assignment.FileTypeId is not null)
        {
            RefTermDto? fileTypeRefTerm = await _metaDataClient.GetByIdAsync(assignment.FileTypeId.Value, cancellationToken);
            if (fileTypeRefTerm is null)
                throw new BadRequestException("The expected file type for this assignment is not recognized.");

            String extension = Path.GetExtension(request.FileName);
            String expectedExtension = $".{fileTypeRefTerm.Key}".ToLowerInvariant();

            if (!extension.Equals(expectedExtension, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(
                    $"Invalid file type. Only {fileTypeRefTerm.Key} files (.{fileTypeRefTerm.Key.ToLowerInvariant()}) are allowed for this assignment.");
        }

        String uploadDir = _storageOptions.UploadPath;
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        String savedFileName = $"{Guid.NewGuid()}_{request.FileName}";
        String filePath = Path.Combine(uploadDir, savedFileName);

        await using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await request.FileContent.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation("File saved: {FilePath} ({FileSize} bytes)", filePath, request.FileSize);

        RefTermDto? submittedStatus = await _metaDataClient.GetByKeyAsync("EmployeeAssignmentStatus", "Submitted", cancellationToken);

        employeeAssignment.FilePath = filePath;
        employeeAssignment.FileName = request.FileName;
        employeeAssignment.FileSize = request.FileSize;
        employeeAssignment.StatusId = submittedStatus is not null ? Guid.Parse(submittedStatus.Id) : null;

        _programService.UpdateEmployeeAssignment(employeeAssignment);
        await _programService.SaveAsync();

        _logger.LogInformation("EmployeeAssignment {Id} submitted for Assignment {AssignmentId}", request.EmployeeAssignmentId, request.AssignmentId);

        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(employeeAssignment.EnrollmentId);
        if (enrollment is not null && assignment.CreatedBy != Guid.Empty)
        {
            await _notificationClient.SendInAppNotificationAsync(
                assignment.CreatedBy,
                "Assignment Submitted",
                $"Employee has submitted assignment \"{assignment.Title}\".");
        }

        return new SubmitEmployeeAssignmentResponseDto
        {
            Id = employeeAssignment.Id,
            AssignmentId = employeeAssignment.AssignmentId,
            EnrollmentId = employeeAssignment.EnrollmentId,
            FileName = employeeAssignment.FileName,
            FileSize = employeeAssignment.FileSize,
            Status = submittedStatus?.Key
        };
    }
}
