using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Represents a notification sent to a user
/// </summary>
public class Notification : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public Guid? RelatedStudyId { get; set; }
    public DicomStudy? RelatedStudy { get; set; }

    public bool IsRead { get; set; } = false;
    public bool IsSent { get; set; } = false;
}
