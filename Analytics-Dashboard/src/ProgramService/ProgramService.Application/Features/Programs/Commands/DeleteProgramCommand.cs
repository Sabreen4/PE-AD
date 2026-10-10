using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Domain.Entities;
using Shared.Exceptions;

namespace ProgramService.Application.Features.Programs.Commands;

public record DeleteProgramCommand(Int64 ProgramId) : IRequest;

public class DeleteProgramCommandHandler : IRequestHandler<DeleteProgramCommand>
{
    private readonly IProgramService _programService;
    private readonly ILogger<DeleteProgramCommandHandler> _logger;

    public DeleteProgramCommandHandler(IProgramService programService, ILogger<DeleteProgramCommandHandler> logger)
    {
        _programService = programService;
        _logger = logger;
    }

    public async Task Handle(DeleteProgramCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting program: {ProgramId}", request.ProgramId);

        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.ProgramId);
        if (program is null)
        {
            _logger.LogWarning("Program not found: {ProgramId}", request.ProgramId);
            throw new NotFoundException("Program", request.ProgramId);
        }

        await _programService.DeleteProgramAsync(program);
        await _programService.SaveAsync();

        _logger.LogInformation("Program deleted: {ProgramId}", request.ProgramId);
    }
}