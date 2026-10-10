using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.EmployeeAssignments.Queries;

public record GetAssignmentFileDownloadQuery(long EmployeeAssignmentId) : IRequest<AssignmentFileDownloadResult>;

public class GetAssignmentFileDownloadQueryHandler : IRequestHandler<GetAssignmentFileDownloadQuery, AssignmentFileDownloadResult>
{
    private readonly IProgramService _programService;
    private readonly ILogger<GetAssignmentFileDownloadQueryHandler> _logger;

    public GetAssignmentFileDownloadQueryHandler(
        IProgramService programService,
        ILogger<GetAssignmentFileDownloadQueryHandler> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    public async Task<AssignmentFileDownloadResult> Handle(GetAssignmentFileDownloadQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing file download for EmployeeAssignmentId: {EmployeeAssignmentId}", request.EmployeeAssignmentId);

        EmployeeAssignment? employeeAssignment = await _programService.GetEmployeeAssignmentByIdAsync(request.EmployeeAssignmentId);
        if (employeeAssignment is null)
        {
            _logger.LogWarning("EmployeeAssignment not found: {EmployeeAssignmentId}", request.EmployeeAssignmentId);
            throw new NotFoundException("EmployeeAssignment", request.EmployeeAssignmentId);
        }

        if (string.IsNullOrEmpty(employeeAssignment.FilePath))
        {
            _logger.LogWarning("EmployeeAssignment {EmployeeAssignmentId} has no file path", request.EmployeeAssignmentId);
            throw new NotFoundException("File", $"EmployeeAssignment {request.EmployeeAssignmentId}");
        }

        string filePath = employeeAssignment.FilePath;

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("File not found on disk for EmployeeAssignmentId: {EmployeeAssignmentId}, path: {FilePath}", request.EmployeeAssignmentId, filePath);
            throw new NotFoundException("File", $"EmployeeAssignment {request.EmployeeAssignmentId}");
        }

        var fileInfo = new System.IO.FileInfo(filePath);
        if (fileInfo.Length != employeeAssignment.FileSize)
        {
            _logger.LogWarning(
                "File size mismatch for EmployeeAssignmentId: {EmployeeAssignmentId}. Expected: {ExpectedSize}, Actual: {ActualSize}",
                request.EmployeeAssignmentId, employeeAssignment.FileSize, fileInfo.Length);
        }

        _logger.LogInformation(
            "Downloading assignment file. EmployeeAssignmentId: {EmployeeAssignmentId}, FilePath: {FilePath}, FileName: {FileName}, FileSize: {FileSize}",
            request.EmployeeAssignmentId, filePath, employeeAssignment.FileName, fileInfo.Length);

        string contentType = GetContentType(filePath);

        return new AssignmentFileDownloadResult
        {
            FilePath = filePath,
            FileName = employeeAssignment.FileName,
            ContentType = contentType
        };
    }

    private static string GetContentType(string filePath)
    {
        string? ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            _ => "application/octet-stream"
        };
    }
}
