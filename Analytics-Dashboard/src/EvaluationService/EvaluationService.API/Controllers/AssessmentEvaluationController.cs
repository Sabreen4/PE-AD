using EvaluationService.Application.Dtos.AssessmentEvaluation;
using EvaluationService.Application.Features.AssessmentEvaluation.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace EvaluationService.API.Controllers;

/// <summary>
/// API controller for retrieving assessment evaluation details for an employee assignment.
/// </summary>
[ApiController]
[Route("employee-assignment/{employeeAssignmentId:long}/assessment-evaluation")]
public class AssessmentEvaluationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AssessmentEvaluationController> _logger;

    public AssessmentEvaluationController(IMediator mediator, ILogger<AssessmentEvaluationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the assessment evaluation details for the specified employee assignment.
    /// </summary>
    /// <param name="employeeAssignmentId">The employee assignment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The assessment evaluation details if found.</returns>
    [HttpGet]
    [FeatureAuthorize("EVAL_ASSESSMENT_EVAL_READ")]
    public async Task<ActionResult<AssessmentEvaluationDetailDto?>> GetById(
        Int64 employeeAssignmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting assessment evaluation for employee assignment {EmployeeAssignmentId}", employeeAssignmentId);
        AssessmentEvaluationDetailDto? result = await _mediator.Send(
            new GetAssessmentEvaluationQuery(employeeAssignmentId), cancellationToken);
        return Ok(result);
    }
}
