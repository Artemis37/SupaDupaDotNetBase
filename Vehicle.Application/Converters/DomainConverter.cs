using Vehicle.Application.Dtos;

namespace Vehicle.Application.Converters;

public static class DomainConverter
{
    public static VehicleDto Map(Domain.Models.Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            PersonId = vehicle.PersonId,
            Type = vehicle.Type,
            LicensePlate = vehicle.LicensePlate,
            CreatedAt = vehicle.CreatedAt
        };
    }

    public static List<VehicleDto> Map(IEnumerable<Domain.Models.Vehicle> vehicles)
    {
        return vehicles.Select(Map).ToList();
    }
}
