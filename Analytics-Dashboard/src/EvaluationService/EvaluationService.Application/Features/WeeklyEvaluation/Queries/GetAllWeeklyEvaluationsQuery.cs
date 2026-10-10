using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Common;
using EvaluationService.Application.Dtos.WeeklyEvaluation;
using MediatR;
using Microsoft.Extensions.Logging;
using WEEntity = EvaluationService.Domain.Entities.WeeklyEvaluation;

namespace EvaluationService.Application.Features.WeeklyEvaluation.Queries;

/// <summary>
/// Query to retrieve a paginated list of all weekly evaluations for a given enrollment.
/// </summary>
/// <param name="EnrollmentId">The enrollment identifier.</param>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
public record GetAllWeeklyEvaluationsQuery(Int64 EnrollmentId, Int32 PageNumber, Int32 PageSize) : IRequest<WeeklyEvaluationListResponseDto>;

/// <summary>
/// Handles the <see cref="GetAllWeeklyEvaluationsQuery"/> by fetching and paginating weekly evaluations.
/// </summary>
public class GetAllWeeklyEvaluationsQueryHandler : IRequestHandler<GetAllWeeklyEvaluationsQuery, WeeklyEvaluationListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllWeeklyEvaluationsQueryHandler> _logger;

    public GetAllWeeklyEvaluationsQueryHandler(IEvaluationService evaluationService, IMapper mapper, ILogger<GetAllWeeklyEvaluationsQueryHandler> logger)
    {
        _evaluationService = evaluationService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by fetching all weekly evaluations, paginating them, and mapping to DTOs.
    /// </summary>
    /// <returns>A paginated list of weekly evaluations.</returns>
    public async Task<WeeklyEvaluationListResponseDto> Handle(GetAllWeeklyEvaluationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all weekly evaluations for EnrollmentId: {EnrollmentId}", request.EnrollmentId);
        List<WEEntity> items = await _evaluationService.GetWeeklyEvaluationsByEnrollmentAsync(request.EnrollmentId);

        int totalRecords = items.Count;

        List<WEEntity> paged = items
            .OrderBy(e => e.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<WeeklyEvaluationDto> data = _mapper.Map<List<WeeklyEvaluationDto>>(paged);

        return new WeeklyEvaluationListResponseDto
        {
            Data = data,
            Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, totalRecords)
        };
    }
}
