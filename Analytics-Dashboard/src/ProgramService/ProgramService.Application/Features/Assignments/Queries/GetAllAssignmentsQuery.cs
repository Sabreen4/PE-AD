using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Assignments;
using ProgramService.Domain.Entities;
using Shared.Common.Dtos;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Assignments.Queries;

/// <summary>
/// Query to retrieve a paginated list of all assignments for a given program week.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="ProgramWeekId">The ID of the program week.</param>
/// <param name="PageNumber">The page number.</param>
/// <param name="PageSize">The page size.</param>
public record GetAllAssignmentsQuery(Int64 ProgramId, Int64 ProgramWeekId, Int32 PageNumber, Int32 PageSize, String? Search = null) : IRequest<PagedResponseDto<AssignmentDto>>;

/// <summary>
/// Handles retrieval of paginated assignments with file type metadata.
/// </summary>
public class GetAllAssignmentsQueryHandler : IRequestHandler<GetAllAssignmentsQuery, PagedResponseDto<AssignmentDto>>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<GetAllAssignmentsQueryHandler> _logger;

    public GetAllAssignmentsQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<GetAllAssignmentsQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of paginated assignments.
    /// </summary>
    /// <returns>A paginated response containing assignment data.</returns>
    public async Task<PagedResponseDto<AssignmentDto>> Handle(GetAllAssignmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all assignments for ProgramId: {ProgramId}, ProgramWeekId: {ProgramWeekId}", request.ProgramId, request.ProgramWeekId);
        List<Assignment> allItems = await _programService.GetAssignmentsByWeekAsync(request.ProgramId, request.ProgramWeekId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            allItems = allItems
                .Where(a => a.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || a.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Int32 totalCount = allItems.Count;
        List<Assignment> items = allItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<AssignmentDto> dtos = _mapper.Map<List<AssignmentDto>>(items);
        for (Int32 i = 0; i < items.Count; i++)
        {
            Guid? fileTypeId = items[i].FileTypeId;
            if (fileTypeId.HasValue)
            {
                RefTermDto? fileTypeRefTerm = await _metaDataClient.GetByIdAsync(fileTypeId.GetValueOrDefault(), cancellationToken);
                if (fileTypeRefTerm is not null)
                    dtos[i].FileType = fileTypeRefTerm.Key;
            }
        }

        return new PagedResponseDto<AssignmentDto>
        {
            Data = dtos,
            Pagination = PaginationDto.Create(request.PageNumber, request.PageSize, totalCount)
        };
    }
}
