using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Domain.Entities;
using Shared.Common.Dtos;
using Shared.InterServiceHelper.Grpc.Identity;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Enrollments.Queries;

/// <summary>
/// Query to retrieve a paginated list of enrollments for a given program.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="PageNumber">The page number.</param>
/// <param name="PageSize">The page size.</param>
public record GetAllEnrollmentsQuery(Int64 ProgramId, Int32 PageNumber, Int32 PageSize) : IRequest<PagedResponseDto<EnrollmentDto>>;

/// <summary>
/// Handles retrieval of paginated enrollments with employee names and status metadata.
/// </summary>
public class GetAllEnrollmentsQueryHandler : IRequestHandler<GetAllEnrollmentsQuery, PagedResponseDto<EnrollmentDto>>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityLookupClient;
    private readonly ILogger<GetAllEnrollmentsQueryHandler> _logger;

    public GetAllEnrollmentsQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityLookupClient,
        ILogger<GetAllEnrollmentsQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _identityLookupClient = identityLookupClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of paginated enrollments.
    /// </summary>
    /// <returns>A paginated response containing enrollment data.</returns>
    public async Task<PagedResponseDto<EnrollmentDto>> Handle(GetAllEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all enrollments for ProgramId: {ProgramId}", request.ProgramId);
        List<Enrollment> allItems = await _programService.GetEnrollmentsByProgramAsync(request.ProgramId);
        Int32 totalCount = allItems.Count;
        List<Enrollment> items = allItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<EnrollmentDto> dtos = _mapper.Map<List<EnrollmentDto>>(items);

        var enrichTasks = dtos.Select(async dto =>
        {
            Enrollment? src = items.FirstOrDefault(e => e.Id == dto.Id);
            if (src?.StatusId is not null)
            {
                RefTermDto? status = await _metaDataClient.GetByIdAsync(src.StatusId.Value, cancellationToken);
                if (status is not null)
                    dto.Status = status.Key;
            }

            EmployeeResponse? employee = await _identityLookupClient.GetEmployeeByIdAsync(dto.EmployeeId, cancellationToken);
            if (employee is not null)
                dto.EmployeeName = employee.FullName;
        });
        await Task.WhenAll(enrichTasks);

        return new PagedResponseDto<EnrollmentDto>
        {
            Data = dtos,
            Pagination = PaginationDto.Create(request.PageNumber, request.PageSize, totalCount)
        };
    }
}
