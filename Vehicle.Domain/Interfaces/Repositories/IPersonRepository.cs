using Vehicle.Domain.Models;

namespace Vehicle.Domain.Interfaces.Repositories
{
    public interface IPersonRepository
    {
        /// <summary>
        /// Adds a new Person to the sharding database
        /// </summary>
        /// <param name="person">Person entity to add</param>
        /// <returns>The created Person with generated ID</returns>
        Task<Person> AddAsync(Person person);
    }
}
