using Shared.Application.Interfaces;

namespace Vehicle.Domain.Models
{
    public class Person : IAuditEntity
    {
        // TODO: Create an person syncId
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Audit properties
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
