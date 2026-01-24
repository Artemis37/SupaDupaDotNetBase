using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class PersonRepository : BaseRepository<Person>, IPersonRepository
    {
        public PersonRepository(ShardingDbContext context)
            : base(context)
        {
        }
    }
}
