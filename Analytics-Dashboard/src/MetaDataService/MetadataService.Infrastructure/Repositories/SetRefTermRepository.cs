using MetaDataService.Application.Contracts;
using MetaDataService.Domain.Entities;
using MetaDataService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaDataService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing SetRefTerm junction entities.
/// </summary>
public sealed class SetRefTermRepository : ISetRefTermRepository
{
    private readonly MetaDataDbContext _context;
    private readonly ILogger<SetRefTermRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetRefTermRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public SetRefTermRepository(MetaDataDbContext context, ILogger<SetRefTermRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all SetRefTerm entries associated with the specified RefSet ID.
    /// </summary>
    /// <param name="refSetId">The RefSet identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching SetRefTerm entities with included RefTerm navigation properties.</returns>
    public async Task<List<SetRefTerm>> GetByRefSetIdAsync(Guid refSetId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting SetRefTerms by RefSetId {RefSetId}", refSetId);
        return await _context.SetRefTerms
            .Include(srt => srt.RefTerm)
            .Where(srt => srt.RefSetId == refSetId)
            .ToListAsync(cancellationToken);
    }
}
