using Microsoft.EntityFrameworkCore;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class PersonMasterRepository : MasterBaseRepository<PersonMaster>, IPersonMasterRepository
    {
        public PersonMasterRepository(MasterDbContext context) : base(context)
        {
        }

        public async Task<PersonMaster?> GetByUsernameAsync(string username)
        {
            return await _context.PersonMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Username == username);
        }

        // GetByIdAsync is inherited from MasterBaseRepository<PersonMaster>
    }
}
