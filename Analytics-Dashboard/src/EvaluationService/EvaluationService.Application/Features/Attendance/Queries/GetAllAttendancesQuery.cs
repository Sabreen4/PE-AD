using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Attendance;
using EvaluationService.Application.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using AttEntity = EvaluationService.Domain.Entities.Attendance;

namespace EvaluationService.Application.Features.Attendance.Queries;

/// <summary>
/// Query to retrieve a paginated list of attendance records for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
public record GetAllAttendancesQuery(Int64 EnrollmentId, Int32 PageNumber, Int32 PageSize) : IRequest<AttendanceListResponseDto>;

/// <summary>
/// Handles the <see cref="GetAllAttendancesQuery"/> by fetching and paginating attendance records.
/// </summary>
public class GetAllAttendancesQueryHandler : IRequestHandler<GetAllAttendancesQuery, AttendanceListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllAttendancesQueryHandler> _logger;

    public GetAllAttendancesQueryHandler(IEvaluationService evaluationService, IMapper mapper, ILogger<GetAllAttendancesQueryHandler> logger)
    {
        _evaluationService = evaluationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by fetching all attendance records for the enrollment and paginating them.
    /// </summary>
    /// <returns>A paginated list of attendance records.</returns>
    public async Task<AttendanceListResponseDto> Handle(GetAllAttendancesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all attendances for EnrollmentId: {EnrollmentId}", request.EnrollmentId);
        List<AttEntity> items = await _evaluationService.GetAttendancesByEnrollmentAsync(request.EnrollmentId);

        int totalRecords = items.Count;

        List<AttEntity> paged = items
            .OrderBy(a => a.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<AttendanceDto> data = _mapper.Map<List<AttendanceDto>>(paged);

        return new AttendanceListResponseDto
        {
            Data = data,
            Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, totalRecords)
        };
    }
}
