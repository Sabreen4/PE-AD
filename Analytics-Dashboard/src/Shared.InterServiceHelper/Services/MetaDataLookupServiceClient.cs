using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.InterServiceHelper.Grpc.MetaData;
using Shared.InterServiceHelper.Interfaces;
using GeneratedMetaDataGrpcService = Shared.InterServiceHelper.Grpc.MetaData.MetaDataGrpcService;

namespace Shared.InterServiceHelper.Services;

/// <summary>
/// Provides RefSet -> RefTerms lookups by communicating with MetaDataService via gRPC.
/// This is the only class allowed to reference the generated MetaData gRPC client
/// directly; every other consumer depends on <see cref="IMetaDataLookupServiceClient"/>.
/// </summary>
public class MetaDataLookupServiceClient : IMetaDataLookupServiceClient
    {
        private readonly GeneratedMetaDataGrpcService.MetaDataGrpcServiceClient _client;
        private readonly ILogger<MetaDataLookupServiceClient> _logger;

        public MetaDataLookupServiceClient(
            GeneratedMetaDataGrpcService.MetaDataGrpcServiceClient client,
            ILogger<MetaDataLookupServiceClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IReadOnlyList<RefTermDto>> GetRefTermsBySetAsync(
        string refSetKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requested RefSet: {RefSetKey}", refSetKey);

        try
        {
            GetRefTermsBySetResponse response = await _client.GetRefTermsBySetAsync(
                new GetRefTermsBySetRequest { RefSetKey = refSetKey },
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("RefSet not found: {RefSetKey}", refSetKey);
                return Array.Empty<RefTermDto>();
            }

            return response.RefTerms.ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC RefTerm lookup failed for RefSet {RefSetKey}", refSetKey);
            return Array.Empty<RefTermDto>();
        }
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<RefTermDto>>> GetRefTermsBySetsAsync(
        IEnumerable<string> refSetKeys, CancellationToken cancellationToken = default)
    {
        List<string> keys = refSetKeys.ToList();
        Dictionary<string, IReadOnlyList<RefTermDto>> result = new Dictionary<string, IReadOnlyList<RefTermDto>>();

        if (keys.Count == 0)
        {
            return result;
        }

        try
        {
            GetRefTermsBySetsRequest request = new GetRefTermsBySetsRequest();
            request.RefSetKeys.AddRange(keys);

            GetRefTermsBySetsResponse response = await _client.GetRefTermsBySetsAsync(
                request,
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: cancellationToken);

            foreach (var setResult in response.Results)
            {
                result[setResult.RefSetKey] = setResult.Found
                    ? setResult.RefTerms.ToList()
                    : Array.Empty<RefTermDto>();
            }

            return result;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC RefTerm lookup failed for RefSets {RefSetKeys}", string.Join(", ", keys));

            foreach (var key in keys)
            {
                result[key] = Array.Empty<RefTermDto>();
            }

            return result;
        }
    }

    public async Task<RefTermDto?> GetByKeyAsync(
        string refSetKey, string refTermKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requested RefSet: {RefSetKey}, Requested RefTerm: {RefTermKey}", refSetKey, refTermKey);

        IReadOnlyList<RefTermDto> terms = await GetRefTermsBySetAsync(refSetKey, cancellationToken);
        RefTermDto? matched = terms.FirstOrDefault(t => string.Equals(t.Key, refTermKey, StringComparison.OrdinalIgnoreCase));

        if (matched is null)
        {
            _logger.LogWarning("{RefTermKey} not found in RefSet {RefSetKey}", refTermKey, refSetKey);
        }
        else
        {
            _logger.LogInformation("Resolved RefTermId: {RefTermId} for {RefTermKey}", matched.Id, refTermKey);
        }

        return matched;
    }

    public async Task<RefTermDto?> GetByIdAsync(
        Guid refTermId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requested RefTerm Id: {RefTermId}", refTermId);

        try
        {
            GetRefTermByIdResponse response = await _client.GetRefTermByIdAsync(
                new GetRefTermByIdRequest { RefTermId = refTermId.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("RefTerm not found by Id: {RefTermId}", refTermId);
                return null;
            }

            _logger.LogInformation("Resolved RefTerm by Id: {RefTermId} -> {RefTermKey}", refTermId, response.RefTerm.Key);
            return response.RefTerm;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "gRPC RefTerm lookup failed for Id {RefTermId}", refTermId);
            return null;
        }
    }
}