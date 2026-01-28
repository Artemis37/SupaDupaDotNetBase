namespace Vehicle.Domain.Interfaces.Repositories
{
    public interface IVehicleRepository : IBaseRepository<Models.Vehicle>
    {
        Task<(IEnumerable<Models.Vehicle> Vehicles, int TotalCount)> GetPagedVehiclesAsync(string? searchText, int pageNumber, int pageSize);
    }
}
