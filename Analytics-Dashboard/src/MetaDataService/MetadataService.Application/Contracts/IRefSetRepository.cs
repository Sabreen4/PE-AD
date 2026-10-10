using MetaDataService.Domain.Entities;

namespace MetaDataService.Application.Contracts;

/// <summary>
/// Repository contract for RefSet entities.
/// </summary>
public interface IRefSetRepository
{
    /// <summary>
    /// Retrieves a RefSet by its unique key (name).
    /// </summary>
    /// <param name="key">The key (name) of the RefSet.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching RefSet, or null if not found.</returns>
    Task<RefSet?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all RefSet entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all RefSet entities.</returns>
    Task<List<RefSet>> GetAllAsync(CancellationToken cancellationToken = default);
}
