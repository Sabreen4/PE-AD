using MetaDataService.Application.Contracts;
using MetaDataService.Domain.Entities;
using MetaDataService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaDataService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing RefSet entities.
/// </summary>
public sealed class RefSetRepository : IRefSetRepository
{
    private readonly MetaDataDbContext _context;
    private readonly ILogger<RefSetRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefSetRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public RefSetRepository(MetaDataDbContext context, ILogger<RefSetRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a RefSet by its unique key (name), using a case-insensitive comparison.
    /// </summary>
    /// <param name="key">The key (name) of the RefSet.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching RefSet entity, or null if not found.</returns>
    public async Task<RefSet?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting RefSet by key {Key}", key);
        return await _context.RefSets
            .FirstOrDefaultAsync(rs => rs.Name.ToLower() == key.ToLower(), cancellationToken);
    }


    /// <summary>
    /// Retrieves all RefSet entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all RefSet entities.</returns>
    public async Task<List<RefSet>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all RefSets");
        return await _context.RefSets.ToListAsync(cancellationToken);
    }
}
