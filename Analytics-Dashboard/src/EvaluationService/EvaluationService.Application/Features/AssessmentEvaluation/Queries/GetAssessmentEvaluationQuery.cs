using EvaluationService.Application.Contracts;
using EvaluationService.Application.Dtos.AssessmentEvaluation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.Program;

namespace EvaluationService.Application.Features.AssessmentEvaluation.Queries;

/// <summary>
/// Query to retrieve assessment evaluation details for a given employee assignment.
/// </summary>
/// <param name="EmployeeAssignmentId">The employee assignment identifier.</param>
public record GetAssessmentEvaluationQuery(Int64 EmployeeAssignmentId) : IRequest<AssessmentEvaluationDetailDto?>;

/// <summary>
/// Handles the <see cref="GetAssessmentEvaluationQuery"/> by retrieving assignment details from the Program service.
/// </summary>
public class GetAssessmentEvaluationQueryHandler : IRequestHandler<GetAssessmentEvaluationQuery, AssessmentEvaluationDetailDto?>
{
    private readonly IProgramLookupServiceClient _programClient;
    private readonly ILogger<GetAssessmentEvaluationQueryHandler> _logger;

    public GetAssessmentEvaluationQueryHandler(IProgramLookupServiceClient programClient, ILogger<GetAssessmentEvaluationQueryHandler> logger)
    {
        _programClient = programClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query by retrieving the employee assignment from the Program service and mapping it to an assessment evaluation DTO.
    /// </summary>
    /// <returns>The assessment evaluation details if found.</returns>
    public async Task<AssessmentEvaluationDetailDto?> Handle(GetAssessmentEvaluationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting assessment evaluation for EmployeeAssignmentId: {EmployeeAssignmentId}", request.EmployeeAssignmentId);

        GetEmployeeAssignmentByIdResponse? assignment =
            await _programClient.GetEmployeeAssignmentByIdAsync(request.EmployeeAssignmentId, cancellationToken);
        if (assignment is null)
        {
            _logger.LogWarning("EmployeeAssignment not found: {EmployeeAssignmentId}", request.EmployeeAssignmentId);
            throw new NotFoundException("EmployeeAssignment", request.EmployeeAssignmentId);
        }

        return new AssessmentEvaluationDetailDto
        {
            Id = request.EmployeeAssignmentId,
            Score = assignment.HasMarks ? (decimal?)assignment.Marks : null,
            Remarks = assignment.ReviewerComment ?? string.Empty
        };
    }
}
