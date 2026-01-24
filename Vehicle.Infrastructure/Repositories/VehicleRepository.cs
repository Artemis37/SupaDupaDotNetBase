using Shared.Application.Interfaces;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Domain.Models;

namespace Vehicle.Infrastructure.Repositories
{
    public class VehicleRepository : BaseRepository<Domain.Models.Vehicle>, IVehicleRepository
    {
        public VehicleRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
