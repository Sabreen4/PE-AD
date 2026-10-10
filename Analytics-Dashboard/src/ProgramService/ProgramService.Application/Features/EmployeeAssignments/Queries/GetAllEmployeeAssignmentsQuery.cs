using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Domain.Entities;
using Shared.Common.Dtos;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.EmployeeAssignments.Queries;

public record GetAllEmployeeAssignmentsQuery(Int64 ProgramId, Int64 ProgramWeekId, Int64 AssignmentId, string? Search, Int32 PageNumber, Int32 PageSize) : IRequest<PagedResponseDto<EmployeeAssignmentEnrichedDto>>;

public class GetAllEmployeeAssignmentsQueryHandler : IRequestHandler<GetAllEmployeeAssignmentsQuery, PagedResponseDto<EmployeeAssignmentEnrichedDto>>
{
    private readonly IProgramService _programService;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly ILogger<GetAllEmployeeAssignmentsQueryHandler> _logger;

    public GetAllEmployeeAssignmentsQueryHandler(
        IProgramService programService,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityClient,
        ILogger<GetAllEmployeeAssignmentsQueryHandler> logger)
    {
        _programService = programService;
        _metaDataClient = metaDataClient;
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<PagedResponseDto<EmployeeAssignmentEnrichedDto>> Handle(GetAllEmployeeAssignmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee assignments for AssignmentId: {AssignmentId}, Search: '{Search}'", request.AssignmentId, request.Search);

        List<EmployeeAssignment> allItems = string.IsNullOrWhiteSpace(request.Search)
            ? await _programService.GetEmployeeAssignmentsByAssignmentAsync(request.AssignmentId)
            : await _programService.GetEmployeeAssignmentsByAssignmentSearchAsync(request.AssignmentId, request.Search);

        var distinctEnrollmentIds = allItems.Select(ea => ea.EnrollmentId).Distinct().ToList();
        var enrollments = await _programService.GetEnrollmentsByIdsAsync(distinctEnrollmentIds);
        var enrollmentMap = enrollments.ToDictionary(e => e.Id);

        var distinctProgramIds = enrollments.Select(e => e.TrainingProgramId).Distinct().ToList();
        var programs = await _programService.GetProgramsByIdsAsync(distinctProgramIds);
        var programMap = programs.ToDictionary(p => p.Id);

        var distinctAssignmentIds = allItems.Select(ea => ea.AssignmentId).Distinct().ToList();
        var assignmentLookupTasks = distinctAssignmentIds.Select(id => _programService.GetAssignmentByIdAsync(id));
        var assignments = await Task.WhenAll(assignmentLookupTasks);
        var assignmentMap = assignments.Where(a => a is not null).Cast<Assignment>().ToDictionary(a => a.Id);

        var distinctWeekIds = assignmentMap.Values.Where(a => a.ProgramWeekId.HasValue).Select(a => a.ProgramWeekId!.Value).Distinct().ToList();
        var weeks = await _programService.GetProgramWeeksByIdsAsync(distinctWeekIds);
        var weekMap = weeks.ToDictionary(w => w.Id);

        var distinctEmployeeIds = enrollments.Select(e => e.EmployeeId).Distinct().ToList();
        var employeeTasks = distinctEmployeeIds.Select(id => _identityClient.GetEmployeeByIdAsync(id, cancellationToken));
        var employeeResponses = await Task.WhenAll(employeeTasks);
        var employeeMap = new Dictionary<Guid, (string Name, string Email)>();
        for (int i = 0; i < distinctEmployeeIds.Count; i++)
        {
            var response = employeeResponses[i];
            if (response is not null)
                employeeMap[distinctEmployeeIds[i]] = (response.FullName ?? string.Empty, response.Email ?? string.Empty);
        }

        List<EmployeeAssignmentEnrichedDto> allDtos = allItems.Select(ea =>
        {
            enrollmentMap.TryGetValue(ea.EnrollmentId, out Enrollment? enrollment);
            programMap.TryGetValue(enrollment?.TrainingProgramId ?? 0, out TrainingPrograms? program);
            assignmentMap.TryGetValue(ea.AssignmentId, out Assignment? assignment);
            weekMap.TryGetValue(assignment?.ProgramWeekId ?? 0, out ProgramWeek? week);
            (string Name, string Email) employee = (string.Empty, string.Empty);
            if (enrollment is not null)
                employeeMap.TryGetValue(enrollment.EmployeeId, out employee);

            return new EmployeeAssignmentEnrichedDto
            {
                EmployeeAssignmentId = ea.Id,
                EmployeeId = enrollment?.EmployeeId ?? Guid.Empty,
                EmployeeName = employee.Name,
                EmployeeEmail = employee.Email,
                ProgramId = program?.Id ?? 0,
                ProgramName = program?.Name ?? string.Empty,
                ProgramWeekId = week?.Id ?? 0,
                ProgramWeekName = week is not null ? $"Week {week.WeekNumber} - {week.Title}" : string.Empty,
                AssignmentId = ea.AssignmentId,
                AssignmentTitle = assignment?.Title ?? string.Empty,
                EnrollmentId = ea.EnrollmentId,
                Status = null,
                FileName = ea.FileName,
                FileSize = ea.FileSize,
                Marks = ea.Marks,
                Comment = ea.ReviewerComment,
                SubmittedAt = ea.CreatedAt
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search;
            allDtos = allDtos.Where(d =>
                d.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.ProgramName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.AssignmentTitle.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.FileName.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        Int32 totalCount = allDtos.Count;

        List<EmployeeAssignmentEnrichedDto> pagedDtos = allDtos
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        foreach (EmployeeAssignmentEnrichedDto dto in pagedDtos)
        {
            EmployeeAssignment? src = allItems.FirstOrDefault(ea => ea.Id == dto.EmployeeAssignmentId);
            if (src?.StatusId is not null)
            {
                RefTermDto? status = await _metaDataClient.GetByIdAsync(src.StatusId.Value, cancellationToken);
                if (status is not null)
                    dto.Status = status.Key;
            }
        }

        return new PagedResponseDto<EmployeeAssignmentEnrichedDto>
        {
            Data = pagedDtos,
            Pagination = PaginationDto.Create(request.PageNumber, request.PageSize, totalCount)
        };
    }
}
