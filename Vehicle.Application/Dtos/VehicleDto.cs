using Vehicle.Domain.Enums;

namespace Vehicle.Application.Dtos;

public class VehicleDto
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public VehicleType Type { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
