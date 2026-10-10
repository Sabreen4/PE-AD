using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.Enrollments.Commands;

/// <summary>
/// Command to delete an enrollment.
/// </summary>
/// <param name="ProgramId">The ID of the training program.</param>
/// <param name="EnrollmentId">The ID of the enrollment to delete.</param>
public record DeleteEnrollmentCommand(Int64 ProgramId, Int64 EnrollmentId) : IRequest;

/// <summary>
/// Handles the deletion of an enrollment with validation.
/// </summary>
public class DeleteEnrollmentCommandHandler : IRequestHandler<DeleteEnrollmentCommand>
{
    private readonly IProgramService _programService;
    private readonly ILogger<DeleteEnrollmentCommandHandler> _logger;

    public DeleteEnrollmentCommandHandler(IProgramService programService, ILogger<DeleteEnrollmentCommandHandler> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the enrollment deletion process.
    /// </summary>
    public async Task Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting enrollment: {EnrollmentId} for ProgramId: {ProgramId}", request.EnrollmentId, request.ProgramId);

        Enrollment? enrollment = await _programService.GetEnrollmentByIdAsync(request.EnrollmentId);
        if (enrollment is null || enrollment.TrainingProgramId != request.ProgramId)
        {
            _logger.LogWarning("Enrollment not found: {EnrollmentId}", request.EnrollmentId);
            throw new NotFoundException("Enrollment", request.EnrollmentId);
        }

        _programService.DeleteEnrollment(enrollment);
        await _programService.SaveAsync();
    }
}
