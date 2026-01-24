using Vehicle.Domain.Enums;

namespace Vehicle.Domain.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        
        // Navigation property
        public Person? Person { get; set; }
    }
}
