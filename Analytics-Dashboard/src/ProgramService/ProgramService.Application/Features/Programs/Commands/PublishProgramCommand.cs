using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Programs.Commands;

/// <summary>
/// Command to publish a training program, notifying all enrolled employees.
/// </summary>
/// <param name="Id">The ID of the program to publish.</param>
public record PublishProgramCommand(Int64 Id) : IRequest<ProgramDetailDto>;

/// <summary>
/// Handles publishing a program, updating status and sending notifications to enrolled employees.
/// </summary>
public class PublishProgramCommandHandler : IRequestHandler<PublishProgramCommand, ProgramDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly INotificationGrpcClient _notificationClient;
    private readonly ILogger<PublishProgramCommandHandler> _logger;

    public PublishProgramCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        INotificationGrpcClient notificationClient,
        ILogger<PublishProgramCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the program publishing process.
    /// </summary>
    /// <returns>The published program detail data.</returns>
    public async Task<ProgramDetailDto> Handle(PublishProgramCommand request, CancellationToken cancellationToken)
    {
        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.Id);
        if (program is null)
            throw new NotFoundException("Program", request.Id);

        RefTermDto? publishedStatus = await _metaDataClient.GetByKeyAsync("ProgramStatus", "Published", cancellationToken);
        if (publishedStatus is not null)
            program.StatusId = Guid.Parse(publishedStatus.Id);

        _programService.UpdateProgram(program);
        await _programService.SaveAsync();

        _logger.LogInformation("Program published: Id={ProgramId}, Name={Name}", program.Id, program.Name);

        List<Enrollment> enrollments = await _programService.GetEnrollmentsByProgramAsync(request.Id);
        foreach (Enrollment enrollment in enrollments.Where(e => e.IsActive))
        {
            await _notificationClient.SendInAppNotificationAsync(enrollment.EmployeeId, "Program Published", $"Program \"{program.Name}\" has been published.");
        }

        ProgramDetailDto result = _mapper.Map<ProgramDetailDto>(program);
        if (program.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(program.StatusId.Value, cancellationToken);
            if (status is not null)
                result.Status = status.Key;
        }

        return result;
    }
}
