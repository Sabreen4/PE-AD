using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using ProgramService.Application.Contracts;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Domain.Entities;
using Shared.Exceptions;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;

namespace ProgramService.Application.Features.Programs.Queries;

/// <summary>
/// Query to retrieve a specific training program by its ID.
/// </summary>
/// <param name="Id">The ID of the program.</param>
public record GetProgramQuery(Int64 Id) : IRequest<ProgramDetailDto>;

/// <summary>
/// Handles retrieval of a single program with status metadata.
/// </summary>
public class GetProgramQueryHandler : IRequestHandler<GetProgramQuery, ProgramDetailDto>
{
    private readonly IProgramService _programService;
    private readonly IMapper _mapper;
    private readonly IMetaDataLookupServiceClient _metaDataClient;
    private readonly ILogger<GetProgramQueryHandler> _logger;

    public GetProgramQueryHandler(
        IProgramService programService,
        IMapper mapper,
        IMetaDataLookupServiceClient metaDataClient,
        ILogger<GetProgramQueryHandler> logger)
    {
        _programService = programService;
        _mapper = mapper;
        _metaDataClient = metaDataClient;
        _logger = logger;
    }

    /// <summary>
    /// Handles retrieval of the program.
    /// </summary>
    /// <returns>The program detail data.</returns>
    public async Task<ProgramDetailDto> Handle(GetProgramQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting program: {ProgramId}", request.Id);

        TrainingPrograms? program = await _programService.GetProgramByIdAsync(request.Id);
        if (program is null)
        {
            _logger.LogWarning("Program not found: {ProgramId}", request.Id);
            throw new NotFoundException("Program", request.Id);
        }

        ProgramDetailDto dto = _mapper.Map<ProgramDetailDto>(program);
        if (program.StatusId is not null)
        {
            RefTermDto? status = await _metaDataClient.GetByIdAsync(program.StatusId.Value, cancellationToken);
            if (status is not null)
                dto.Status = status.Key;
        }

        return dto;
    }
}
