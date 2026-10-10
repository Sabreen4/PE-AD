using MetaDataService.Application.Contracts;
using MetaDataService.Domain.Entities;
using MetaDataService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaDataService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing RefTerm entities.
/// </summary>
public sealed class RefTermRepository : IRefTermRepository
{
    private readonly MetaDataDbContext _context;
    private readonly ILogger<RefTermRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefTermRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public RefTermRepository(MetaDataDbContext context, ILogger<RefTermRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all RefTerms associated with the specified RefSet ID, optionally filtered by active status.
    /// </summary>
    /// <param name="refSetId">The RefSet identifier.</param>
    /// <param name="isActive">Optional filter for active or inactive terms.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching RefTerm entities.</returns>
    public async Task<List<RefTerm>> GetBySetIdAsync(Guid refSetId, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting RefTerms by SetId {RefSetId}", refSetId);
        IQueryable<SetRefTerm> query = _context.SetRefTerms
            .Include(srt => srt.RefTerm)
            .Where(srt => srt.RefSetId == refSetId);

        if (isActive.HasValue)
            query = query.Where(srt => srt.RefTerm.IsActive == isActive.Value);

        return await query
            .Select(srt => srt.RefTerm)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a single RefTerm by its unique identifier.
    /// </summary>
    /// <param name="id">The RefTerm identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching RefTerm entity, or null if not found.</returns>
    public async Task<RefTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting RefTerm by id {Id}", id);
        return await _context.RefTerms.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }
}
