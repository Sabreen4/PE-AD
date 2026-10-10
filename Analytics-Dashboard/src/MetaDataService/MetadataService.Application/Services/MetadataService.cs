using AutoMapper;
using MetaDataService.Application.Contracts;
using MetaDataService.Application.Dtos;
using MetaDataService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Common.Dtos;
using Shared.Exceptions;
using Shared.Redis.Interfaces;

namespace MetaDataService.Application.Services;

/// <summary>
/// Application service for retrieving reference data (RefSet/RefTerm) terms
/// with pagination and active filtering support.
/// </summary>
public sealed class MetadataService : IMetadataService
{
    private readonly IRepoWrapper _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<MetadataService> _logger;
    private readonly ICacheService _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataService"/> class.
    /// </summary>
    /// <param name="repo">The repository wrapper.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cache">The Redis cache service.</param>
    public MetadataService(IRepoWrapper repo, IMapper mapper, ILogger<MetadataService> logger, ICacheService cache)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Retrieves paginated RefSet terms for the given key, with optional active filtering and AutoMapper projection.
    /// Checks the Redis cache first; on miss, fetches from the database and populates the cache.
    /// </summary>
    /// <param name="key">The RefSet key (name).</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="isActive">Optional filter for active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing RefSet term DTOs.</returns>
    public async Task<PagedResponseDto<RefSetTermResponseDto>> GetRefSetTermsAsync(
        string key, int page = 1, int pageSize = 20, bool? isActive = null, string? search = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"refset_terms:{key}:page:{page}:size:{pageSize}:active:{isActive}:search:{search}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                _logger.LogInformation("Cache miss for {CacheKey}, fetching from database", cacheKey);
                _logger.LogInformation("Getting RefSet terms for key {RefSetKey}, page {Page}, pageSize {PageSize}", key, page, pageSize);

                RefSet? refSet = await _repo.RefSets.GetByKeyAsync(key, ct);

                if (refSet is null)
                {
                    _logger.LogWarning("RefSet not found: {RefSetKey}", key);
                    throw new NotFoundException("RefSet", key);
                }

                List<RefTerm> terms = await _repo.RefTerms.GetBySetIdAsync(refSet.Id, isActive, ct);

                List<RefSetTermResponseDto> dtos = _mapper.Map<List<RefSetTermResponseDto>>(terms)
                    .Select(d => d with { RefSetId = refSet.Id })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    dtos = dtos
                        .Where(d => d.Key.Contains(search, StringComparison.OrdinalIgnoreCase)
                            || d.Value.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                int totalCount = dtos.Count;
                List<RefSetTermResponseDto> paged = dtos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Returning {Count} RefSet terms for key {RefSetKey} (total {TotalCount})", paged.Count, key, totalCount);

                return new PagedResponseDto<RefSetTermResponseDto>
                {
                    Data = paged,
                    Pagination = PaginationDto.Create(page, pageSize, totalCount)
                };
            },
            cancellationToken: cancellationToken);
    }
}
