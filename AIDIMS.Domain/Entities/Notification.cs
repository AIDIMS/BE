using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class Notification : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public Guid? RelatedStudyId { get; set; }
    public DicomStudy? RelatedStudy { get; set; }

    public Guid? RelatedVisitId { get; set; }
    public PatientVisit? RelatedVisit { get; set; }

    public bool IsRead { get; set; } = false;
    public bool IsSent { get; set; } = false;
}
