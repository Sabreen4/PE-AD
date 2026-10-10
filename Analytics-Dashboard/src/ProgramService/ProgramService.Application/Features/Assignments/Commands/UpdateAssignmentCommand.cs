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
/// Command to update an existing assignment.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="ProgramWeekId">The ID of the program week.</param>
/// <param name="AssignmentId">The ID of the assignment to update.</param>
/// <param name="Dto">The assignment update data.</param>
public record UpdateAssignmentCommand(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, UpdateAssignmentDto Dto) : IRequest<AssignmentDetailDto>;

/// <summary>
/// Handles the update of an existing assignment, including file type resolution.
/// </summary>
public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, AssignmentDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<UpdateAssignmentCommandHandler> _logger;

    public UpdateAssignmentCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<UpdateAssignmentCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the assignment update process.
    /// </summary>
    /// <returns>The updated assignment detail data.</returns>
    public async Task<AssignmentDetailDto> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating assignment: {AssignmentId} for ProgramId: {ProgramId}, ProgramWeekId: {ProgramWeekId}", request.AssignmentId, request.ProgramId, request.ProgramWeekId);

        Assignment? assignment = await _programService.GetAssignmentByProgramWeekAndIdAsync(request.ProgramId, request.ProgramWeekId, request.AssignmentId);
        if (assignment is null)
        {
            _logger.LogWarning("Assignment not found: {AssignmentId}", request.AssignmentId);
            throw new NotFoundException("Assignment", request.AssignmentId);
        }

        assignment.Title = request.Dto.Title;
        assignment.Description = request.Dto.Description;
        assignment.StartDate = request.Dto.StartDate;
        assignment.EndDate = request.Dto.EndDate;
        assignment.MaxMarks = request.Dto.MaxMarks;
        assignment.FilePath = request.Dto.FilePath ?? string.Empty;
        assignment.FileTypeId = null;

        if (!string.IsNullOrEmpty(request.Dto.FileType))
        {
            RefTermDto? fileTypeRefTerm = await _metaDataClient.GetByKeyAsync("FileType", request.Dto.FileType, cancellationToken);
            if (fileTypeRefTerm is null)
            {
                _logger.LogWarning("Invalid FileType: {FileType}", request.Dto.FileType);
                throw new BadRequestException($"Invalid FileType '{request.Dto.FileType}'.");
            }
            assignment.FileTypeId = Guid.Parse(fileTypeRefTerm.Id);
        }

        _programService.UpdateAssignment(assignment);
        await _programService.SaveAsync();

        return _mapper.Map<AssignmentDetailDto>(assignment);
    }
}
