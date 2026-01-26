using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        public PersonRepository(ShardingDbContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
        }

        public async Task<Person?> GetByPersonSyncIdAsync(Guid personSyncId)
        {
            return await Context.Set<Person>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonSyncId == personSyncId);
        }
    }
}
