using Vehicle.Domain.Enums;

namespace Vehicle.Application.Dtos;

public class UpdateVehicleRequest
{
    public VehicleType Type { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
}
