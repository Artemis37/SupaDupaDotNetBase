using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class VehicleRepository : BaseRepository<Domain.Models.Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ShardingDbContext context)
            : base(context)
        {
        }
    }
}
