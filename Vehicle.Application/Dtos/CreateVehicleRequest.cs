using Vehicle.Domain.Enums;

namespace Vehicle.Application.Dtos;

public class CreateVehicleRequest
{
    public VehicleType Type { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
}
