using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Interfaces;

namespace EvaluationService.Infrastructure.Persistence;

/// <summary>
/// Base repository providing common CRUD operations for any entity type.
/// </summary>
public abstract class RepositoryBase<T> : IRepositoryBase<T>
    where T : class
{
    protected readonly EvaluationDbContext Context;
    protected readonly ILogger<RepositoryBase<T>> Logger;

    protected RepositoryBase(EvaluationDbContext context, ILogger<RepositoryBase<T>> logger)
    {
        Context = context;
        Logger = logger;
    }

    /// <summary>
    /// Returns a queryable of all entities of type T with no tracking.
    /// </summary>
    /// <returns>An <see cref="IQueryable{T}"/> of all entities.</returns>
    public IQueryable<T> FindAll()
    {
        Logger.LogInformation("Finding all {EntityType}", typeof(T).Name);
        return Context.Set<T>().AsNoTracking();
    }

    /// <summary>
    /// Returns a queryable of entities matching the given expression with no tracking.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of matching entities.</returns>
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        Logger.LogInformation("Finding {EntityType} by condition", typeof(T).Name);
        return Context.Set<T>().Where(expression).AsNoTracking();
    }

    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public async Task<T?> GetByIdAsync(object id)
    {
        Logger.LogInformation("Getting {EntityType} by id {Id}", typeof(T).Name, id);
        return await Context.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Adds a new entity to the context.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    public async Task CreateAsync(T entity)
    {
        Logger.LogInformation("Creating {EntityType}", typeof(T).Name);
        await Context.Set<T>().AddAsync(entity);
    }

    /// <summary>
    /// Marks an entity as updated in the context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity)
    {
        Logger.LogInformation("Updating {EntityType}", typeof(T).Name);
        Context.Set<T>().Update(entity);
    }

    /// <summary>
    /// Marks an entity as deleted in the context.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(T entity)
    {
        Logger.LogInformation("Deleting {EntityType}", typeof(T).Name);
        Context.Set<T>().Remove(entity);
    }

    /// <summary>
    /// Checks whether any entity matches the given expression.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>True if any matching entity exists; otherwise false.</returns>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        Logger.LogInformation("Checking existence of {EntityType}", typeof(T).Name);
        return await Context.Set<T>().AnyAsync(expression);
    }

    /// <summary>
    /// Checks whether any entity matches the given expression, optionally ignoring global query filters.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global query filters.</param>
    /// <returns>True if any matching entity exists; otherwise false.</returns>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression, bool ignoreQueryFilters)
    {
        Logger.LogInformation("Checking existence of {EntityType} (ignoreQueryFilters: {Ignore})", typeof(T).Name, ignoreQueryFilters);
        IQueryable<T> query = Context.Set<T>();
        if (ignoreQueryFilters)
            query = query.IgnoreQueryFilters();
        return await query.AnyAsync(expression);
    }
}
