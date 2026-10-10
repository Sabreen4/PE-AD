using MetaDataService.Domain.Entities;

namespace MetaDataService.Application.Contracts;

/// <summary>
/// Repository contract for RefTerm entities.
/// </summary>
public interface IRefTermRepository
{
    /// <summary>
    /// Retrieves RefTerms by the specified RefSet ID, optionally filtered by active status.
    /// </summary>
    /// <param name="refSetId">The RefSet identifier.</param>
    /// <param name="isActive">Optional filter for active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching RefTerm entities.</returns>
    Task<List<RefTerm>> GetBySetIdAsync(Guid refSetId, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single RefTerm by its unique identifier.
    /// </summary>
    /// <param name="id">The RefTerm identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching RefTerm, or null if not found.</returns>
    Task<RefTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
