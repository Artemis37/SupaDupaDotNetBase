namespace Shared.Application.Interfaces
{
    public interface IAuditEntity
    {
        int? CreatedBy { get; set; }
        DateTime? CreatedAt { get; set; }
        int? UpdatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}
