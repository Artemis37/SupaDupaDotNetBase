using Vehicle.Domain.Models;

namespace Vehicle.Domain.Interfaces.Repositories
{
    public interface IPersonRepository : IBaseRepository<Person>
    {
        /// <summary>
        /// Gets a Person by PersonSyncId
        /// </summary>
        /// <param name="personSyncId">PersonSyncId to search for</param>
        /// <returns>Person if found, null otherwise</returns>
        Task<Person?> GetByPersonSyncIdAsync(Guid personSyncId);
    }
}
