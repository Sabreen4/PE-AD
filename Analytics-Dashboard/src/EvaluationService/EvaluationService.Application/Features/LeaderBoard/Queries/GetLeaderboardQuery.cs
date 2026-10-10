using AutoMapper;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.Common;
using EvaluationService.Application.Dtos.Leaderboard;
using EvaluationService.Application.Features.Leaderboard.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.Program;
using LbEntity = EvaluationService.Domain.Entities.Leaderboard;

namespace EvaluationService.Application.Features.Leaderboard.Queries;

/// <summary>
/// Query to retrieve a paginated leaderboard for a given program.
/// </summary>
/// <param name="ProgramId">The program identifier.</param>
/// <param name="PageNumber">The page number to retrieve.</param>
/// <param name="PageSize">The number of items per page.</param>
public record GetLeaderboardQuery(Int64 ProgramId, Int32 PageNumber, Int32 PageSize) : IRequest<LeaderboardListResponseDto>;

/// <summary>
/// Handles the <see cref="GetLeaderboardQuery"/> by filtering leaderboard entries for the specified program and paginating results.
/// </summary>
public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardListResponseDto>
{
    private readonly IEvaluationService _evaluationService;
    private readonly IProgramLookupServiceClient _programClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLeaderboardQueryHandler> _logger;

    public GetLeaderboardQueryHandler(
        IEvaluationService evaluationService,
        IProgramLookupServiceClient programClient,
        IIdentityLookupServiceClient identityClient,
        IMediator mediator,
        IMapper mapper,
        ILogger<GetLeaderboardQueryHandler> logger)
    {
        _evaluationService = evaluationService;
        _programClient = programClient;
        _identityClient = identityClient;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by filtering leaderboard entries for the program, paginating, and enriching with enrollment data.
    /// </summary>
    /// <returns>A paginated leaderboard for the program.</returns>
    public async Task<LeaderboardListResponseDto> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting leaderboard for ProgramId: {ProgramId}", request.ProgramId);
        List<LbEntity> allItems = await _evaluationService.GetAllLeaderboardsAsync();

        if (allItems.Count == 0)
        {
            _logger.LogInformation("Leaderboard is empty — auto-recalculating for ProgramId: {ProgramId}", request.ProgramId);
            return await _mediator.Send(new RecalculateLeaderboardCommand(request.ProgramId), cancellationToken);
        }

        var enrollmentTasks = allItems.Select(item => _programClient.GetEnrollmentByIdAsync(item.EnrollmentId, cancellationToken));
        GetEnrollmentResponse?[] enrollmentResults = await Task.WhenAll(enrollmentTasks);

        List<LbEntity> programItems = new();
        for (int i = 0; i < allItems.Count; i++)
        {
            GetEnrollmentResponse? enrollment = enrollmentResults[i];
            if (enrollment is not null && enrollment.ProgramId == request.ProgramId)
            {
                programItems.Add(allItems[i]);
            }
        }

        if (programItems.Count == 0)
        {
            _logger.LogInformation("No leaderboard entries found for ProgramId: {ProgramId} — auto-recalculating", request.ProgramId);
            return await _mediator.Send(new RecalculateLeaderboardCommand(request.ProgramId), cancellationToken);
        }

        programItems = programItems.OrderBy(l => l.Rank).ToList();

        int totalRecords = programItems.Count;

        List<LbEntity> paged = programItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        List<LeaderboardEntryDto> data = _mapper.Map<List<LeaderboardEntryDto>>(paged);

        var enrichTasks = data.Select(async entry =>
        {
            GetEnrollmentResponse? enrollment = await _programClient.GetEnrollmentByIdAsync(entry.EnrollmentId, cancellationToken);
            if (enrollment is not null)
            {
                entry.EmployeeId = Guid.Parse(enrollment.EmployeeId);
                var employee = await _identityClient.GetEmployeeByIdAsync(entry.EmployeeId, cancellationToken);
                entry.EmployeeName = employee?.FullName ?? string.Empty;
            }

            entry.Percentile = totalRecords > 1
                ? Math.Round((decimal)(totalRecords - entry.Rank) / (totalRecords - 1) * 100, 2)
                : 100m;
        });
        await Task.WhenAll(enrichTasks);

        return new LeaderboardListResponseDto
        {
            Data = data,
            Pagination = PaginationMetadataDto.Create(request.PageNumber, request.PageSize, totalRecords)
        };
    }
}
