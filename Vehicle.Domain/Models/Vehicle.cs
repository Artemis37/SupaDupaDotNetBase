using Shared.Application.Interfaces;
using Vehicle.Domain.Enums;

namespace Vehicle.Domain.Models
{
    public class Vehicle : IPersonEntity, IAuditEntity
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        
        // Audit properties
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation property
        public Person? Person { get; set; }
    }
}
