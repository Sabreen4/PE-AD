using MetaDataService.Domain.Entities;

namespace MetaDataService.Application.Contracts;

/// <summary>
/// Repository contract for SetRefTerm junction entities.
/// </summary>
public interface ISetRefTermRepository
{
    /// <summary>
    /// Retrieves all SetRefTerm entries by the specified RefSet ID.
    /// </summary>
    /// <param name="refSetId">The RefSet identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching SetRefTerm entities.</returns>
    Task<List<SetRefTerm>> GetByRefSetIdAsync(Guid refSetId, CancellationToken cancellationToken = default);
}
