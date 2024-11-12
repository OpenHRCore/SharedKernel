using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OpenHRCore.SharedKernel.Domain
{
    /// <summary>
    /// Defines a generic repository interface for performing CRUD (Create, Read, Update, Delete) and aggregate operations on entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity this repository handles. Must inherit from OpenHRCoreBaseEntity.</typeparam>
    public interface IOpenHRCoreBaseRepository<TEntity> 
        where TEntity : OpenHRCoreBaseEntity
    {
        #region Create Operations

        /// <summary>
        /// Adds a single entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
        void Add(TEntity entity);

        /// <summary>
        /// Adds multiple entities to the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to add. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Asynchronously adds a single entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Asynchronously adds multiple entities to the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to add. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        #endregion

        #region Read Operations

        /// <summary>
        /// Asynchronously retrieves all entities from the repository.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Asynchronously retrieves all entities from the repository with specified related entities included.
        /// </summary>
        /// <param name="includeProperties">Expressions specifying the related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities with included properties.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// Asynchronously retrieves a page of entities from the repository with sorting, filtering and searching capabilities.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="orderBy">A function to order the entities.</param>
        /// <param name="ascending">True to sort in ascending order, false for descending order.</param>
        /// <param name="searchCriteria">Dictionary containing property names and search values for filtering.</param>
        /// <param name="includeProperties">Expressions specifying the related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with total count and filtered/sorted collection.</returns>
        Task<(int TotalCount, IEnumerable<TEntity> Items)> GetPagedAsync<TKey>(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, TKey>>? orderBy = null,
            bool ascending = true,
            Dictionary<string, string>? searchCriteria = null,
            params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// Asynchronously retrieves entities that match the specified predicate.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Asynchronously retrieves entities that match the specified predicate and includes related entities.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <param name="includeProperties">A comma-separated list of related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the condition, including specified related entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties);

        /// <summary>
        /// Asynchronously retrieves entities that match the specified predicate and includes related entities.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <param name="includeProperties">Expressions specifying the related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the condition, including specified related entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// Asynchronously retrieves an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve. Must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity with the specified identifier, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
        Task<TEntity> GetByIdAsync(object id);

        /// <summary>
        /// Asynchronously retrieves the first entity that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the condition, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Asynchronously retrieves the first entity that matches the specified predicate and includes related entities.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <param name="includeProperties">A comma-separated list of related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the condition, including specified related entities, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties);

        /// <summary>
        /// Asynchronously retrieves the first entity that matches the specified predicate and includes related entities.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. Must not be null.</param>
        /// <param name="includeProperties">Expressions specifying the related entities to include.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the condition, including specified related entities, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates a single entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
        void Update(TEntity entity);

        /// <summary>
        /// Updates multiple entities in the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to update. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
        void UpdateRange(IEnumerable<TEntity> entities);

        #endregion

        #region Delete Operations

        /// <summary>
        /// Removes a single entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
        void Remove(TEntity entity);

        /// <summary>
        /// Removes an entity with the specified identifier from the repository.
        /// </summary>
        /// <param name="id">The identifier of the entity to remove. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
        void Remove(object id);

        /// <summary>
        /// Removes multiple entities from the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to remove. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when entities collection is null.</exception>
        void RemoveRange(IEnumerable<TEntity> entities);

        #endregion

        #region Aggregate Operations

        /// <summary>
        /// Returns the maximum value of a property in the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of the property to find the maximum value for.</typeparam>
        /// <param name="selector">A function to extract the property to compare. Must not be null.</param>
        /// <returns>The maximum value of the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no elements.</exception>
        TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector);

        /// <summary>
        /// Asynchronously returns the maximum value of a property in the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of the property to find the maximum value for.</typeparam>
        /// <param name="selector">A function to extract the property to compare. Must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the maximum value of the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when selector is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the sequence contains no elements.</exception>
        Task<TResult> MaxAsync<TResult>(Expression<Func<TEntity, TResult>> selector);

        #endregion
    }
}
