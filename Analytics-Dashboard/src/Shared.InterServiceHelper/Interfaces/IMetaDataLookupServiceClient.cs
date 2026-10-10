using Shared.InterServiceHelper.Grpc.MetaData;

namespace Shared.InterServiceHelper.Interfaces;

/// <summary>
/// Resolves RefTerms for RefSets a calling service owns, by talking to
/// MetaDataService's gRPC endpoint, which is the only place RefSet/RefTerm
/// data is queried from.
/// </summary>
public interface IMetaDataLookupServiceClient
{
    /// <summary>
    /// Returns the RefTerms belonging to a single RefSet.
    /// Returns an empty list if the RefSet doesn't exist or the call fails.
    /// </summary>
    Task<IReadOnlyList<RefTermDto>> GetRefTermsBySetAsync(string refSetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the RefTerms belonging to each requested RefSet in one round trip.
    /// RefSet keys that don't exist (or if the call fails entirely) map to an empty list.
    /// </summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<RefTermDto>>> GetRefTermsBySetsAsync(
        IEnumerable<string> refSetKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single RefTerm within a RefSet by its key.
    /// Returns null if the RefSet doesn't exist, the RefTerm key isn't found in it,
    /// or the underlying gRPC call fails.
    /// </summary>
    Task<RefTermDto?> GetByKeyAsync(
        string refSetKey, string refTermKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single RefTerm by its Id (Guid).
    /// Returns null if the RefTerm doesn't exist or the underlying gRPC call fails.
    /// </summary>
    Task<RefTermDto?> GetByIdAsync(
        Guid refTermId, CancellationToken cancellationToken = default);
}