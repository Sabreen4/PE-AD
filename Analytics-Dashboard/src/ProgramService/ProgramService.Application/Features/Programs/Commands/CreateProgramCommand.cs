using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Domain.Entities;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Programs.Commands;

/// <summary>
/// Command to create a new training program.
/// </summary>
/// <param name="Dto">The program creation data.</param>
/// <param name="EvaluatorId">The ID of the evaluator.</param>
/// <param name="OrganizationId">The ID of the organization.</param>
public record CreateProgramCommand(CreateProgramDto Dto, Guid EvaluatorId, Int64 OrganizationId) : IRequest<ProgramCreatedDto>;

/// <summary>
/// Handles the creation of a training program, including status resolution from metadata.
/// </summary>
public class CreateProgramCommandHandler : IRequestHandler<CreateProgramCommand, ProgramCreatedDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<CreateProgramCommandHandler> _logger;

    public CreateProgramCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<CreateProgramCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the program creation process.
    /// </summary>
    /// <returns>The created program data.</returns>
    public async Task<ProgramCreatedDto> Handle(CreateProgramCommand request, CancellationToken cancellationToken)
    {
        Guid? statusId = null;
        if (!String.IsNullOrWhiteSpace(request.Dto.Status))
        {
            try
            {
                RefTermDto? statusRefTerm = await _metaDataClient.GetByKeyAsync("ProgramStatus", request.Dto.Status, cancellationToken);
                if (statusRefTerm is not null)
                    statusId = Guid.Parse(statusRefTerm.Id);
                else
                    _logger.LogWarning("Program status '{Status}' not found in Metadata Service. StatusId will be null.", request.Dto.Status);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve program status '{Status}' from Metadata Service. StatusId will be null.", request.Dto.Status);
            }
        }

        TrainingPrograms program = new TrainingPrograms
        {
            OrganizationId = request.OrganizationId,
            EvaluatorId = request.EvaluatorId,
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            DurationWeeks = request.Dto.DurationWeeks,
            StatusId = statusId
        };

        await _programService.CreateProgramAsync(program);
        await _programService.SaveAsync();

        _logger.LogInformation("Program created: Id={ProgramId}, Name={Name}", program.Id, program.Name);

        return new ProgramCreatedDto
        {
            Id = program.Id,
            OrganizationId = program.OrganizationId,
            Name = program.Name
        };
    }
}
