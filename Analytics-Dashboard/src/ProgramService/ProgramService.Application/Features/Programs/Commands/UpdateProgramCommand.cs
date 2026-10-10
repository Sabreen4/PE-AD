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
/// Command to update an existing training program.
/// </summary>
/// <param name="Id">The ID of the program to update.</param>
/// <param name="Dto">The program update data.</param>
public record UpdateProgramCommand(Int64 Id, UpdateProgramDto Dto) : IRequest<ProgramDetailDto>;

/// <summary>
/// Handles the update of a training program, including status resolution from metadata.
/// </summary>
public class UpdateProgramCommandHandler : IRequestHandler<UpdateProgramCommand, ProgramDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<UpdateProgramCommandHandler> _logger;

    public UpdateProgramCommandHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<UpdateProgramCommandHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles the program update process.
    /// </summary>
    /// <returns>The updated program detail data.</returns>
    public async Task<ProgramDetailDto> Handle(UpdateProgramCommand request, CancellationToken cancellationToken)
    {
        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.Id);
        if (program is null)
            throw new NotFoundException("Program", request.Id);

        program.Name = request.Dto.Name;
        program.Description = request.Dto.Description;
        program.DurationWeeks = request.Dto.DurationWeeks;

        if (!String.IsNullOrWhiteSpace(request.Dto.Status))
        {
            try
            {
                RefTermDto? statusRefTerm = await _metaDataClient.GetByKeyAsync("ProgramStatus", request.Dto.Status, cancellationToken);
                if (statusRefTerm is not null)
                    program.StatusId = Guid.Parse(statusRefTerm.Id);
                else
                    _logger.LogWarning("Program status '{Status}' not found in Metadata Service. Status unchanged.", request.Dto.Status);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve program status '{Status}' from Metadata Service. Status unchanged.", request.Dto.Status);
            }
        }

        _programService.UpdateProgram(program);
        await _programService.SaveAsync();

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
