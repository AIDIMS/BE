using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Represents a DICOM Study (một ca chụp)
/// </summary>
public class DicomStudy : BaseAuditableEntity
{
    public Guid OrderId { get; set; }
    public ImagingOrder Order { get; set; } = default!;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public string OrthancStudyId { get; set; } = string.Empty;
    public string StudyUid { get; set; } = string.Empty;
    public string? StudyDescription { get; set; }
    public string? AccessionNumber { get; set; }
    public Modality Modality { get; set; } = default!;
    public DateTime StudyDate { get; set; }

    public Guid AssignedDoctorId { get; set; }
    public User AssignedDoctor { get; set; } = default!;

    // Navigation properties
    public ICollection<DicomSeries> Series { get; set; } = new List<DicomSeries>();
    public AiResult? AiResult { get; set; }
    public Diagnosis? Diagnosis { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
