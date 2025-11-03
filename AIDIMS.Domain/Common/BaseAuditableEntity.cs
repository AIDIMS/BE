namespace AIDIMS.Domain.Common;

/// <summary>
/// Base auditable entity with tracking information
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
