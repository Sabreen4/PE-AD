using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Domain.Entities;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Programs.Queries;

public record GetAllProgramsQuery(Int32 PageNumber, Int32 PageSize, String? Search = null) : IRequest<PagedResponseDto<ProgramDto>>;

public class GetAllProgramsQueryHandler : IRequestHandler<GetAllProgramsQuery, PagedResponseDto<ProgramDto>>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<GetAllProgramsQueryHandler> _logger;

    public GetAllProgramsQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        IUserContextService userContext,
        ILogger<GetAllProgramsQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<PagedResponseDto<ProgramDto>> Handle(GetAllProgramsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all programs");

        List<TrainingPrograms> allItems = await _programService.GetAllProgramsAsync();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            allItems = allItems
                .Where(p => p.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        bool isAdmin = _userContext.Roles.Contains("Admin");
        bool isEvaluator = _userContext.Roles.Contains("Evaluator");
        bool isEmployee = _userContext.Roles.Contains("Employee");

        if (isEvaluator)
        {
            allItems = allItems
                .Where(p => p.OrganizationId == _userContext.OrganizationId && p.EvaluatorId == _userContext.UserId)
                .ToList();
        }
        else if (isEmployee)
        {
            List<long> enrolledProgramIds = await _programService.GetEnrolledProgramIdsAsync(_userContext.UserId!.Value);
            allItems = allItems
                .Where(p => enrolledProgramIds.Contains(p.Id))
                .ToList();
        }
        else if (!isAdmin)
        {
            allItems = allItems
                .Where(p => p.OrganizationId == _userContext.OrganizationId)
                .ToList();
        }

        Int32 totalCount = allItems.Count;
        List<TrainingPrograms> items = allItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<ProgramDto> dtos = _mapper.Map<List<ProgramDto>>(items);
        for (Int32 i = 0; i < items.Count; i++)
        {
            Guid? statusId = items[i].StatusId;
            if (statusId.HasValue)
            {
                RefTermDto? status = await _metaDataClient.GetByIdAsync(statusId.Value, cancellationToken);
                if (status is not null)
                    dtos[i].Status = status.Key;
            }
        }

        return new PagedResponseDto<ProgramDto>
        {
            Data = dtos,
            Pagination = PaginationDto.Create(request.PageNumber, request.PageSize, totalCount)
        };
    }
}
