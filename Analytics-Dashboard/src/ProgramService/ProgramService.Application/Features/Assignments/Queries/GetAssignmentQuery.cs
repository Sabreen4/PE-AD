using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Assignments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Assignments.Queries;

/// <summary>
/// Query to retrieve a specific assignment by its ID within a program and week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="ProgramWeekId">The ID of the program week.</param>
/// <param name="AssignmentId">The ID of the assignment.</param>
public record GetAssignmentQuery(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId) : IRequest<AssignmentDetailDto>;

/// <summary>
/// Handles retrieval of a single assignment with file type metadata.
/// </summary>
public class GetAssignmentQueryHandler : IRequestHandler<GetAssignmentQuery, AssignmentDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<GetAssignmentQueryHandler> _logger;

    public GetAssignmentQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<GetAssignmentQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of the assignment.
    /// </summary>
    /// <returns>The assignment detail data.</returns>
    public async Task<AssignmentDetailDto> Handle(GetAssignmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting assignment: {AssignmentId} for ProgramId: {ProgramId}, ProgramWeekId: {ProgramWeekId}", request.AssignmentId, request.ProgramId, request.ProgramWeekId);

        Assignment? assignment = await _programService.GetAssignmentByProgramWeekAndIdAsync(request.ProgramId, request.ProgramWeekId, request.AssignmentId);
        if (assignment is null)
        {
            _logger.LogWarning("Assignment not found: {AssignmentId}", request.AssignmentId);
            throw new NotFoundException("Assignment", request.AssignmentId);
        }

        AssignmentDetailDto dto = _mapper.Map<AssignmentDetailDto>(assignment);
        if (assignment.FileTypeId.HasValue)
        {
            RefTermDto? fileTypeRefTerm = await _metaDataClient.GetByIdAsync(assignment.FileTypeId.Value, cancellationToken);
            if (fileTypeRefTerm is not null)
                dto.FileType = fileTypeRefTerm.Key;
        }

        return dto;
    }
}
