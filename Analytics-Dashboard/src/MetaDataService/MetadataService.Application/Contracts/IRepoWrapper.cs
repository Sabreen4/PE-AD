namespace MetaDataService.Application.Contracts;

/// <summary>
/// Unit-of-work contract that exposes all metadata repositories and a save method.
/// </summary>
public interface IRepoWrapper
{
    /// <summary>Gets the RefSet repository.</summary>
    IRefSetRepository RefSets { get; }

    /// <summary>Gets the RefTerm repository.</summary>
    IRefTermRepository RefTerms { get; }

    /// <summary>Gets the SetRefTerm repository.</summary>
    ISetRefTermRepository SetRefTerms { get; }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveAsync();
}
