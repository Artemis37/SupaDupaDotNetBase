using Shared.Application.Context;
using Shared.Application.Interfaces;
using Vehicle.Application.Decorators;
using Vehicle.Application.Dtos;
using Vehicle.Domain.Interfaces.Repositories;

namespace Vehicle.Application.Queries;

public class GetAllVehiclesQuery : IQuery<List<VehicleDto>>
{
}

[LoggingQuery]
public class GetAllVehiclesQueryHandler : IQueryHandler<GetAllVehiclesQuery, List<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly PersonContext _personContext;

    public GetAllVehiclesQueryHandler(IVehicleRepository vehicleRepository, PersonContext personContext)
    {
        _vehicleRepository = vehicleRepository;
        _personContext = personContext;
    }

    public async Task<List<VehicleDto>> Handle(GetAllVehiclesQuery query)
    {
        // Get all vehicles for current person
        var vehicles = await _vehicleRepository.GetAllAsync();
        
        // Filter by PersonId from context
        var filteredVehicles = vehicles
            .Where(v => v.PersonId == _personContext.PersonId && !v.IsDeleted)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                PersonId = v.PersonId,
                Type = v.Type,
                LicensePlate = v.LicensePlate,
                CreatedAt = v.CreatedAt
            })
            .ToList();

        return filteredVehicles;
    }
}
