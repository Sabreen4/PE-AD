using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Domain.Entities;
using Shared.Authorization.Interfaces;
using Shared.Common.Dtos;
using Shared.Exceptions;

namespace ProgramService.Application.Features.ProgramWeeks.Queries;

public record GetAllProgramWeeksQuery(Int64 ProgramId, Int32 PageNumber, Int32 PageSize, String? Search = null) : IRequest<PagedResponseDto<ProgramWeekDto>>;

public class GetAllProgramWeeksQueryHandler : IRequestHandler<GetAllProgramWeeksQuery, PagedResponseDto<ProgramWeekDto>>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContext;
    private readonly ILogger<GetAllProgramWeeksQueryHandler> _logger;

    public GetAllProgramWeeksQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IUserContextService userContext,
        ILogger<GetAllProgramWeeksQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<PagedResponseDto<ProgramWeekDto>> Handle(GetAllProgramWeeksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all program weeks for ProgramId: {ProgramId}", request.ProgramId);

        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
        if (program is null)
        {
            _logger.LogWarning("Program not found: {ProgramId}", request.ProgramId);
            throw new NotFoundException("Program", request.ProgramId);
        }

        bool isAdmin = _userContext.Roles.Contains("Admin");
        bool isEvaluator = _userContext.Roles.Contains("Evaluator");
        bool isEmployee = _userContext.Roles.Contains("Employee");

        if (isEvaluator && program.EvaluatorId != _userContext.UserId)
        {
            _logger.LogWarning("Evaluator {UserId} does not own program {ProgramId}", _userContext.UserId, request.ProgramId);
            throw new NotFoundException("Program", request.ProgramId);
        }

        if (isEmployee)
        {
            bool enrolled = await _programService.IsEmployeeEnrolledAsync(request.ProgramId, _userContext.UserId!.Value);
            if (!enrolled)
            {
                _logger.LogWarning("Employee {UserId} is not enrolled in program {ProgramId}", _userContext.UserId, request.ProgramId);
                throw new NotFoundException("Program", request.ProgramId);
            }
        }

        List<ProgramWeek> allItems = await _programService.GetProgramWeeksByProgramAsync(request.ProgramId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            allItems = allItems
                .Where(pw => pw.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || pw.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        List<ProgramWeek> items = allItems
            .OrderBy(w => w.WeekNumber)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResponseDto<ProgramWeekDto>
        {
            Data = _mapper.Map<List<ProgramWeekDto>>(items),
            Pagination = PaginationDto.Create(request.PageNumber, request.PageSize, allItems.Count)
        };
    }
}
