using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.Enrollments.Queries;

/// <summary>
/// Query to retrieve the enrollment of the currently authenticated employee for a given program.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="UserId">The ID of the authenticated user (from JWT).</param>
public record GetMyEnrollmentQuery(Int64 ProgramId, Guid UserId) : IRequest<EnrollmentDetailDto>;

/// <summary>
/// Handles retrieval of the current employee's own enrollment without requiring ENROLLMENT_VIEW.
/// No gRPC calls are made; the employee is already identified via the JWT.
/// </summary>
public class GetMyEnrollmentQueryHandler : IRequestHandler<GetMyEnrollmentQuery, EnrollmentDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IIdentityLookupServiceClient _identityClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyEnrollmentQueryHandler> _logger;

    public GetMyEnrollmentQueryHandler(
        IProgramService programService,
        IIdentityLookupServiceClient identityClient,
        IMapper mapper,
        ILogger<GetMyEnrollmentQueryHandler> logger)
    {
        _programService = programService;
        _identityClient = identityClient;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by looking up the active enrollment for the given program and user.
    /// </summary>
    /// <returns>The enrollment detail data.</returns>
    public async Task<EnrollmentDetailDto> Handle(GetMyEnrollmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting my enrollment for ProgramId: {ProgramId}, UserId: {UserId}",
            request.ProgramId, request.UserId);

        Enrollment? enrollment = await _programService.GetActiveEnrollmentByEmployeeAsync(
            request.ProgramId, request.UserId);

        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found for ProgramId: {ProgramId}, UserId: {UserId}",
                request.ProgramId, request.UserId);
            throw new NotFoundException("Enrollment", request.ProgramId);
        }

        EnrollmentDetailDto dto = _mapper.Map<EnrollmentDetailDto>(enrollment);

        var employee = await _identityClient.GetEmployeeByIdAsync(dto.EmployeeId, cancellationToken);
        dto.EmployeeName = employee?.FullName;

        _logger.LogInformation("Successfully retrieved enrollment {EnrollmentId} for user {UserId}",
            dto.Id, request.UserId);

        return dto;
    }
}
