using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Identity;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Enrollments.Queries;

/// <summary>
/// Query to retrieve a specific enrollment by its ID.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="EnrollmentId">The ID of the enrollment.</param>
public record GetEnrollmentQuery(Int64 ProgramId, Int64 EnrollmentId) : IRequest<EnrollmentDetailDto>;

/// <summary>
/// Handles retrieval of a single enrollment with employee name and status metadata.
/// </summary>
public class GetEnrollmentQueryHandler : IRequestHandler<GetEnrollmentQuery, EnrollmentDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly IIdentityLookupServiceClient _identityLookupClient;
    private readonly ILogger<GetEnrollmentQueryHandler> _logger;

    public GetEnrollmentQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        IIdentityLookupServiceClient identityLookupClient,
        ILogger<GetEnrollmentQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _identityLookupClient = identityLookupClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of the enrollment.
    /// </summary>
    /// <returns>The enrollment detail data.</returns>
    public async Task<EnrollmentDetailDto> Handle(GetEnrollmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting enrollment: {EnrollmentId} for ProgramId: {ProgramId}", request.EnrollmentId, request.ProgramId);

        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(request.EnrollmentId);
        if (enrollment is null)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
            throw new NotFoundException("Enrollment", request.EnrollmentId);
        }

        EnrollmentDetailDto dto = _mapper.Map<EnrollmentDetailDto>(enrollment);
        if (enrollment.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(enrollment.StatusId.Value, cancellationToken);
            if (status is not null)
                dto.Status = status.Key;
        }

        EmployeeResponse? employee = await _identityLookupClient.GetEmployeeByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee is not null)
            dto.EmployeeName = employee.FullName;

        return dto;
    }
}
