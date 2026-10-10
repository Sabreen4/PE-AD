using MetaDataService.Application.Contracts;
using MetaDataService.Infrastructure.Persistence;
using MetaDataService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Interfaces;

namespace MetaDataService.Infrastructure.Common;

/// <summary>
/// Wraps all metadata repositories into a single unit of work.
/// </summary>
public sealed class RepoWrapper : IRepoWrapper
{
    private readonly MetaDataDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly ILogger<RepoWrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private IRefSetRepository? _refSets;
    private IRefTermRepository? _refTerms;
    private ISetRefTermRepository? _setRefTerms;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepoWrapper"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="userContext">The user context service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating sub-repository loggers.</param>
    public RepoWrapper(MetaDataDbContext context, IUserContextService userContext, ILogger<RepoWrapper> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets the RefSet repository.
    /// </summary>
    public IRefSetRepository RefSets
    {
        get { return _refSets ??= new RefSetRepository(_context, _loggerFactory.CreateLogger<RefSetRepository>()); }
    }

    /// <summary>
    /// Gets the RefTerm repository.
    /// </summary>
    public IRefTermRepository RefTerms
    {
        get { return _refTerms ??= new RefTermRepository(_context, _loggerFactory.CreateLogger<RefTermRepository>()); }
    }

    /// <summary>
    /// Gets the SetRefTerm repository.
    /// </summary>
    public ISetRefTermRepository SetRefTerms
    {
        get { return _setRefTerms ??= new SetRefTermRepository(_context, _loggerFactory.CreateLogger<SetRefTermRepository>()); }
    }

    /// <summary>
    /// Persists all pending changes to the database with audit fields applied.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveAsync()
    {
        _logger.LogInformation("Saving changes to database");
        Guid currentUserId = _userContext.UserId ?? Guid.Empty;
        int result = await _context.SaveAsync(currentUserId);
        _logger.LogInformation("Saved {Count} changes to database", result);
        return result;
    }
}
