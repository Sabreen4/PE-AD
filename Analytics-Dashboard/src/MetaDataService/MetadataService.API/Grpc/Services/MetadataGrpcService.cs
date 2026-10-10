using Grpc.Core;
using MetaDataService.Application.Contracts;
using MetaDataService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Redis.Interfaces;
using S_MetaData = Shared.InterServiceHelper.Grpc.MetaData;

namespace MetaDataService.API.Grpc.Services;

/// <summary>
/// gRPC service for exposing metadata (RefSet/RefTerm) lookups to other services.
/// </summary>
public sealed class MetadataGrpcService : S_MetaData.MetaDataGrpcService.MetaDataGrpcServiceBase
{
    private readonly IRepoWrapper _repo;
    private readonly ILogger<MetadataGrpcService> _logger;
    private readonly ICacheService _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataGrpcService"/> class.
    /// </summary>
    /// <param name="repo">The repository wrapper.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cache">The Redis cache service.</param>
    public MetadataGrpcService(IRepoWrapper repo, ILogger<MetadataGrpcService> logger, ICacheService cache)
    {
        _repo = repo;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Retrieves all reference terms for a given RefSet key via gRPC.
    /// Checks the Redis cache first; on miss, fetches from the database and populates the cache.
    /// </summary>
    /// <param name="request">The gRPC request containing the RefSet key.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A gRPC response indicating whether terms were found and listing them.</returns>
    public override async Task<S_MetaData.GetRefTermsBySetResponse> GetRefTermsBySet(
        S_MetaData.GetRefTermsBySetRequest request,
        ServerCallContext context)
    {
        try
        {
            string refSetCacheKey = $"refset:{request.RefSetKey}";
            string termsCacheKey = $"refset_terms:{request.RefSetKey}:all";

            string? refSetId = await _cache.GetOrSetAsync<string>(
                refSetCacheKey,
                async ct =>
                {
                    _logger.LogInformation("Cache miss for {CacheKey}, fetching RefSet from database", refSetCacheKey);
                    RefSet? refSet = await _repo.RefSets.GetByKeyAsync(request.RefSetKey, ct);
                    if (refSet is null)
                    {
                        _logger.LogWarning("RefSet not found: {RefSetKey}", request.RefSetKey);
                        return string.Empty;
                    }
                    return refSet.Id.ToString();
                },
                cancellationToken: context.CancellationToken);

            if (string.IsNullOrEmpty(refSetId))
            {
                return new S_MetaData.GetRefTermsBySetResponse { Found = false };
            }

            List<S_MetaData.RefTermDto> cachedTerms = await _cache.GetOrSetAsync<List<S_MetaData.RefTermDto>>(
                termsCacheKey,
                async ct =>
                {
                    _logger.LogInformation("Cache miss for {CacheKey}, fetching RefTerms from database", termsCacheKey);
                    List<RefTerm> terms = await _repo.RefTerms.GetBySetIdAsync(Guid.Parse(refSetId), cancellationToken: ct);
                    return terms.Select(t => new S_MetaData.RefTermDto
                    {
                        Id = t.Id.ToString(),
                        Key = t.Name,
                        Description = t.Description
                    }).ToList();
                },
                cancellationToken: context.CancellationToken);

            S_MetaData.GetRefTermsBySetResponse response = new S_MetaData.GetRefTermsBySetResponse
            {
                Found = true
            };
            response.RefTerms.AddRange(cachedTerms);

            _logger.LogInformation("Resolved {Count} RefTerms for RefSet: {RefSetKey}", cachedTerms.Count, request.RefSetKey);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC GetRefTermsBySet failed for key {RefSetKey}", request.RefSetKey);
            return new S_MetaData.GetRefTermsBySetResponse { Found = false };
        }
    }

    /// <summary>
    /// Retrieves a single reference term by its unique identifier via gRPC.
    /// Checks the Redis cache first; on miss, fetches from the database and populates the cache.
    /// </summary>
    /// <param name="request">The gRPC request containing the RefTerm ID.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A gRPC response indicating whether the term was found and its details.</returns>
    public override async Task<S_MetaData.GetRefTermByIdResponse> GetRefTermById(
        S_MetaData.GetRefTermByIdRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.RefTermId, out Guid refTermId))
            {
                _logger.LogWarning("Invalid RefTermId format: {RefTermId}", request.RefTermId);
                return new S_MetaData.GetRefTermByIdResponse { Found = false };
            }

            string cacheKey = $"refterm:{refTermId}";

            S_MetaData.RefTermDto? refTermDto = await _cache.GetOrSetAsync<S_MetaData.RefTermDto>(
                cacheKey,
                async ct =>
                {
                    _logger.LogInformation("Cache miss for {CacheKey}, fetching from database", cacheKey);
                    RefTerm? refTerm = await _repo.RefTerms.GetByIdAsync(refTermId, ct);
                    if (refTerm is null)
                    {
                        _logger.LogWarning("RefTerm not found: {RefTermId}", refTermId);
                        return null!;
                    }
                    return new S_MetaData.RefTermDto
                    {
                        Id = refTerm.Id.ToString(),
                        Key = refTerm.Name,
                        Description = refTerm.Description
                    };
                },
                cancellationToken: context.CancellationToken);

            if (refTermDto is null)
            {
                return new S_MetaData.GetRefTermByIdResponse { Found = false };
            }

            return new S_MetaData.GetRefTermByIdResponse
            {
                Found = true,
                RefTerm = refTermDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC GetRefTermById failed for Id {RefTermId}", request.RefTermId);
            return new S_MetaData.GetRefTermByIdResponse { Found = false };
        }
    }

    /// <summary>
    /// Retrieves reference terms for multiple RefSet keys in a single gRPC call.
    /// Checks the Redis cache first per key; on miss, fetches from the database and populates the cache.
    /// </summary>
    /// <param name="request">The gRPC request containing a list of RefSet keys.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A gRPC response with results for each requested RefSet key.</returns>
    public override async Task<S_MetaData.GetRefTermsBySetsResponse> GetRefTermsBySets(
        S_MetaData.GetRefTermsBySetsRequest request,
        ServerCallContext context)
    {
        try
        {
            S_MetaData.GetRefTermsBySetsResponse response = new S_MetaData.GetRefTermsBySetsResponse();

            foreach (string key in request.RefSetKeys)
            {
                string refSetCacheKey = $"refset:{key}";
                string termsCacheKey = $"refset_terms:{key}:all";

                string? refSetId = await _cache.GetOrSetAsync<string>(
                    refSetCacheKey,
                    async ct =>
                    {
                        _logger.LogInformation("Cache miss for {CacheKey}, fetching RefSet from database", refSetCacheKey);
                        RefSet? refSet = await _repo.RefSets.GetByKeyAsync(key, ct);
                        return refSet?.Id.ToString() ?? string.Empty;
                    },
                    cancellationToken: context.CancellationToken);

                if (string.IsNullOrEmpty(refSetId))
                {
                    response.Results.Add(new S_MetaData.RefSetTerms
                    {
                        RefSetKey = key,
                        Found = false
                    });
                    continue;
                }

                List<S_MetaData.RefTermDto> cachedTerms = await _cache.GetOrSetAsync<List<S_MetaData.RefTermDto>>(
                    termsCacheKey,
                    async ct =>
                    {
                        _logger.LogInformation("Cache miss for {CacheKey}, fetching RefTerms from database", termsCacheKey);
                        List<RefTerm> terms = await _repo.RefTerms.GetBySetIdAsync(Guid.Parse(refSetId), cancellationToken: ct);
                        return terms.Select(t => new S_MetaData.RefTermDto
                        {
                            Id = t.Id.ToString(),
                            Key = t.Name,
                            Description = t.Description
                        }).ToList();
                    },
                    cancellationToken: context.CancellationToken);

                S_MetaData.RefSetTerms setTerms = new S_MetaData.RefSetTerms
                {
                    RefSetKey = key,
                    Found = true
                };
                setTerms.RefTerms.AddRange(cachedTerms);

                _logger.LogInformation("Resolved {Count} RefTerms for RefSet: {RefSetKey}", cachedTerms.Count, key);

                response.Results.Add(setTerms);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC GetRefTermsBySets failed");
            S_MetaData.GetRefTermsBySetsResponse response = new S_MetaData.GetRefTermsBySetsResponse();
            foreach (string key in request.RefSetKeys)
            {
                response.Results.Add(new S_MetaData.RefSetTerms
                {
                    RefSetKey = key,
                    Found = false
                });
            }
            return response;
        }
    }
}
