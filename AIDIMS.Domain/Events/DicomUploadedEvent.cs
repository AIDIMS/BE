namespace AIDIMS.Domain.Events;

/// <summary>
/// Event được publish khi DICOM file được upload thành công
/// </summary>
public class DicomUploadedEvent
{
    public Guid StudyId { get; set; }
    public Guid InstanceId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

