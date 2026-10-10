using System.Linq.Expressions;

namespace Shared.Common.Interfaces;

/// <summary>
/// Defines the contract for generic CRUD operations on an entity type.
/// All repository implementations should adhere to this interface to
/// guarantee consistent data access behaviour across microservices.
/// </summary>
/// <typeparam name="T">The entity type that maps to a database table.</typeparam>
public interface IRepositoryBase<T>
    where T : class
{
    /// <summary>Returns an <see cref="IQueryable{T}"/> for all entities of type <typeparamref name="T"/> with no tracking.</summary>
    IQueryable<T> FindAll();
    /// <summary>Returns an <see cref="IQueryable{T}"/> for entities matching the specified filter expression with no tracking.</summary>
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    /// <summary>Retrieves a single entity by its primary key value.</summary>
    Task<T?> GetByIdAsync(object id);
    /// <summary>Checks whether any entity satisfies the given condition.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    /// <summary>Checks whether any entity satisfies the given condition, optionally ignoring global query filters.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> expression, bool ignoreQueryFilters);
    /// <summary>Adds a new entity to the context asynchronously.</summary>
    Task CreateAsync(T entity);
    /// <summary>Marks an entity as modified so changes are saved on the next SaveChanges call.</summary>
    void Update(T entity);
    /// <summary>Removes an entity from the context.</summary>
    void Delete(T entity);
}
