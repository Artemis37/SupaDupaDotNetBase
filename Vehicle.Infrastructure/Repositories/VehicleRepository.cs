using Microsoft.EntityFrameworkCore;
using Vehicle.Domain.Interfaces.Repositories;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class VehicleRepository : BaseRepository<Domain.Models.Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ShardingDbContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
        }

        public async Task<(IEnumerable<Domain.Models.Vehicle> Vehicles, int TotalCount)> GetPagedVehiclesAsync(string? searchText, int pageNumber, int pageSize)
        {
            var queryable = GetQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // TODO: Extend LicensePlate search to fulltext search (see Vehicle.Domain/Models/Vehicle.cs)
                queryable = queryable.Where(v => v.LicensePlate.ToLower().Contains(searchText.ToLower()));
            }

            var totalCount = await queryable.CountAsync();

            var vehicles = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (vehicles, totalCount);
        }
    }
}
