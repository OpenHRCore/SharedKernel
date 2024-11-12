using Microsoft.EntityFrameworkCore;
using OpenHRCore.SharedKernel.Domain;
using System.Linq.Expressions;

namespace OpenHRCore.SharedKernel.Infrastructure
{
    /// <summary>
    /// Generic repository implementation for handling CRUD operations using Entity Framework Core.
    /// Provides a standard set of database operations for entities that inherit from OpenHRCoreBaseEntity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity. Must inherit from OpenHRCoreBaseEntity.</typeparam>
    public class OpenHRCoreEfBaseRepository<TEntity> : IOpenHRCoreBaseRepository<TEntity>
        where TEntity : OpenHRCoreBaseEntity
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenHRCoreEfBaseRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="dbContext">The Entity Framework database context.</param>
        /// <exception cref="ArgumentNullException">Thrown when dbContext is null.</exception>
        public OpenHRCoreEfBaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region Create Operations

        /// <inheritdoc />
        public void Add(TEntity entity)
        {
            ValidateEntity(entity);
            _dbContext.Set<TEntity>().Add(entity);
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TEntity> entities)
        {
            ValidateEntities(entities);
            _dbContext.Set<TEntity>().AddRange(entities);
        }

        /// <inheritdoc />
        public async Task AddAsync(TEntity entity)
        {
            ValidateEntity(entity);
            await _dbContext.Set<TEntity>().AddAsync(entity);
        }

        /// <inheritdoc />
        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            ValidateEntities(entities);
            await _dbContext.Set<TEntity>().AddRangeAsync(entities);
        }

        #endregion

        #region Read Operations

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbContext.Set<TEntity>().ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ValidatePredicate(predicate);
            return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties)
        {
            ValidatePredicate(predicate);
            var query = BuildQueryWithIncludes(includeProperties);
            return await query.Where(predicate).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            ValidatePredicate(predicate);
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            return await query.Where(predicate).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<TEntity> GetByIdAsync(object id)
        {
            ValidateId(id);
            return await _dbContext.Set<TEntity>().FindAsync(id)
                ?? throw new InvalidOperationException($"Entity with id {id} not found.");
        }

        /// <inheritdoc />
        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ValidatePredicate(predicate);
            return await _dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate)
                ?? throw new InvalidOperationException("Entity not found.");
        }

        /// <inheritdoc />
        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, string includeProperties)
        {
            ValidatePredicate(predicate);
            var query = BuildQueryWithIncludes(includeProperties);
            return await query.FirstOrDefaultAsync(predicate)
                ?? throw new InvalidOperationException("Entity not found.");
        }

        /// <inheritdoc />
        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            ValidatePredicate(predicate);
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            return await query.FirstOrDefaultAsync(predicate)
                ?? throw new InvalidOperationException("Entity not found.");
        }

        /// <summary>
        /// Gets a paged result of entities with optional ordering, filtering and included properties.
        /// </summary>
        /// <typeparam name="TKey">The type of the key used for ordering.</typeparam>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <param name="orderBy">Optional expression to order the results.</param>
        /// <param name="ascending">Whether to order in ascending (true) or descending (false) order.</param>
        /// <param name="searchCriteria">Optional dictionary of property names and search values for filtering.</param>
        /// <param name="includeProperties">Optional related properties to include in the query.</param>
        /// <returns>A tuple containing the total count of items and the paged results.</returns>
        public async Task<(int TotalCount, IEnumerable<TEntity> Items)> GetPagedAsync<TKey>(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, TKey>>? orderBy = null,
            bool ascending = true,
            Dictionary<string, string>? searchCriteria = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            ValidatePagination(pageNumber, pageSize);

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));

            if (searchCriteria?.Any() == true)
            {
                query = ApplySearchCriteria(query, searchCriteria);
            }

            int totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalCount, items);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetPageAsync(int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);
            return await _dbContext.Set<TEntity>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetPageAsync(int pageNumber, int pageSize, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            ValidatePagination(pageNumber, pageSize);
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetPageAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> orderBy, bool ascending = true)
        {
            ValidatePagination(pageNumber, pageSize);
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetPageAsync<TKey>(int pageNumber, int pageSize, Expression<Func<TEntity, TKey>> orderBy, bool ascending = true, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            ValidatePagination(pageNumber, pageSize);
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();
            query = includeProperties.Aggregate(query, (current, property) => current.Include(property));
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        #endregion

        #region Update Operations

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            ValidateEntity(entity);
            _dbContext.Set<TEntity>().Update(entity);
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            ValidateEntities(entities);
            _dbContext.Set<TEntity>().UpdateRange(entities);
        }

        #endregion

        #region Delete Operations

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            ValidateEntity(entity);
            _dbContext.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public void Remove(object id)
        {
            ValidateId(id);
            var entity = _dbContext.Set<TEntity>().Find(id);
            if (entity != null)
            {
                Remove(entity);
            }
        }

        /// <inheritdoc />
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            ValidateEntities(entities);
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        #endregion

        #region Aggregate Operations

        /// <inheritdoc />
        public TResult Max<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            var query = _dbContext.Set<TEntity>();
            return query != null ? query.Max(selector)! : default!;
        }

        /// <inheritdoc />
        public async Task<TResult> MaxAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            ValidateSelector(selector);
            var query = _dbContext.Set<TEntity>();
            return await query.AnyAsync() ? await query.MaxAsync(selector) : default!;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates that the given entity is not null.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
        protected void ValidateEntity(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
        }

        /// <summary>
        /// Validates that the given collection of entities is not null or empty.
        /// </summary>
        /// <param name="entities">The collection of entities to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when entities is null or empty.</exception>
        protected void ValidateEntities(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities));
            }
        }

        /// <summary>
        /// Validates that the given predicate is not null.
        /// </summary>
        /// <param name="predicate">The predicate to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        protected void ValidatePredicate(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
        }

        /// <summary>
        /// Validates that the given id is not null.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
        protected void ValidateId(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        /// <summary>
        /// Validates that the given selector is not null.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">The selector to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when selector is null.</exception>
        protected void ValidateSelector<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
        }

        /// <summary>
        /// Validates pagination parameters.
        /// </summary>
        /// <param name="pageNumber">The page number to validate.</param>
        /// <param name="pageSize">The page size to validate.</param>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        protected void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            }
        }

        /// <summary>
        /// Builds a query with the specified include properties.
        /// </summary>
        /// <param name="includeProperties">A comma-separated list of navigation properties to include.</param>
        /// <returns>An IQueryable with the specified includes.</returns>
        protected IQueryable<TEntity> BuildQueryWithIncludes(string includeProperties)
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query;
        }

        /// <summary>
        /// Applies search criteria to the query using string contains matching.
        /// </summary>
        /// <param name="query">The initial query to apply criteria to.</param>
        /// <param name="searchCriteria">Dictionary containing property names and search values.</param>
        /// <returns>Query with search criteria applied.</returns>
        private static IQueryable<TEntity> ApplySearchCriteria(IQueryable<TEntity> query, Dictionary<string, string> searchCriteria)
        {
            var entityType = typeof(TEntity);
            
            foreach (var criterion in searchCriteria)
            {
                var property = entityType.GetProperty(criterion.Key);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var parameter = Expression.Parameter(entityType, "x");
                    var propertyAccess = Expression.Property(parameter, property);
                    var constant = Expression.Constant(criterion.Value);
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    
                    if (containsMethod != null)
                    {
                        var containsExpression = Expression.Call(propertyAccess, containsMethod, constant);
                        var lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
                        query = query.Where(lambda);
                    }
                }
            }

            return query;
        }

        #endregion
    }
}
