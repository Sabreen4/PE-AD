using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Interfaces;

namespace Shared.Common.Repositories;

/// <summary>
/// Generic base repository providing CRUD operations for entity types.
/// All microservice repositories should inherit from this class to get consistent
/// query tracking, logging, and EF Core state management.
/// </summary>
/// <typeparam name="T">The entity type that maps to a database table.</typeparam>
public class RepositoryBase<T> : IRepositoryBase<T>
    where T : class
{
    /// <summary>The EF Core <see cref="DbContext"/> used for all data access.</summary>
    protected readonly DbContext Context;
    /// <summary>Logger instance for recording repository operations.</summary>
    protected readonly ILogger<RepositoryBase<T>> Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    /// <param name="logger">The logger for this repository.</param>
    public RepositoryBase(DbContext context, ILogger<RepositoryBase<T>> logger)
    {
        Context = context;
        Logger = logger;
    }

    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> for all entities of type <typeparamref name="T"/>
    /// with change tracking disabled.
    /// </summary>
    /// <returns>An <see cref="IQueryable{T}"/> for the full entity set.</returns>
    public IQueryable<T> FindAll()
    {
        Logger.LogInformation("Finding all {EntityType}", typeof(T).Name);
        return Context.Set<T>().AsNoTracking();
    }

    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> for entities matching the specified
    /// filter expression with change tracking disabled.
    /// </summary>
    /// <param name="expression">The filter expression to apply.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of matching entities.</returns>
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        Logger.LogInformation("Finding {EntityType} by condition", typeof(T).Name);
        return Context.Set<T>().Where(expression).AsNoTracking();
    }

    /// <summary>
    /// Retrieves a single entity by its primary key value.
    /// </summary>
    /// <param name="id">The primary key value of the entity.</param>
    /// <returns>The entity if found; otherwise <see langword="null"/>.</returns>
    public async Task<T?> GetByIdAsync(object id)
    {
        Logger.LogInformation("Getting {EntityType} by id {Id}", typeof(T).Name, id);
        return await Context.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Adds a new entity to the context asynchronously. Changes are not persisted
    /// until <c>SaveChanges</c> is called on the <see cref="DbContext"/>.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    public async Task CreateAsync(T entity)
    {
        Logger.LogInformation("Creating {EntityType}", typeof(T).Name);
        await Context.Set<T>().AddAsync(entity);
    }

    /// <summary>
    /// Marks an entity as modified. If the entity is currently detached it is
    /// attached and set to the Modified state so that all properties are updated
    /// on the next <c>SaveChanges</c>.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(T entity)
    {
        Logger.LogInformation("Updating {EntityType}", typeof(T).Name);
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            Context.Set<T>().Attach(entity);
            entry.State = EntityState.Modified;
        }
        else
        {
            Context.Set<T>().Update(entity);
        }
    }

    /// <summary>
    /// Removes an entity from the context. The deletion is not persisted
    /// until <c>SaveChanges</c> is called on the <see cref="DbContext"/>.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(T entity)
    {
        Logger.LogInformation("Deleting {EntityType}", typeof(T).Name);
        Context.Set<T>().Remove(entity);
    }

    /// <summary>
    /// Checks whether any entity of type <typeparamref name="T"/> satisfies the given condition.
    /// </summary>
    /// <param name="expression">The condition to evaluate.</param>
    /// <returns><see langword="true"/> if at least one matching entity exists; otherwise <see langword="false"/>.</returns>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        Logger.LogInformation("Checking existence of {EntityType}", typeof(T).Name);
        return await Context.Set<T>().AnyAsync(expression);
    }

    /// <summary>
    /// Checks whether any entity of type <typeparamref name="T"/> satisfies the given condition,
    /// optionally ignoring global query filters (e.g. soft-delete filters).
    /// </summary>
    /// <param name="expression">The condition to evaluate.</param>
    /// <param name="ignoreQueryFilters">
    /// If <see langword="true"/>, global query filters are bypassed for this check.
    /// </param>
    /// <returns><see langword="true"/> if at least one matching entity exists; otherwise <see langword="false"/>.</returns>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression, bool ignoreQueryFilters)
    {
        Logger.LogInformation("Checking existence of {EntityType} (ignoreQueryFilters: {Ignore})", typeof(T).Name, ignoreQueryFilters);
        IQueryable<T> query = Context.Set<T>();
        if (ignoreQueryFilters)
            query = query.IgnoreQueryFilters();
        return await query.AnyAsync(expression);
    }
}
