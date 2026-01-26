using Vehicle.Domain.Models;

namespace Vehicle.Domain.Interfaces.Repositories
{
    public interface IPersonMasterRepository : IMasterBaseRepository<PersonMaster>
    {
        /// <summary>
        /// Gets a PersonMaster by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>PersonMaster if found, null otherwise</returns>
        Task<PersonMaster?> GetByUsernameAsync(string username);
    }
}
