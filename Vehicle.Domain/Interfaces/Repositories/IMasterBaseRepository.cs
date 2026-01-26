namespace Vehicle.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Base repository interface for entities in the master database
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IMasterBaseRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>The created entity with generated ID</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Checks if an entity exists by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChangesAsync();
    }
}
