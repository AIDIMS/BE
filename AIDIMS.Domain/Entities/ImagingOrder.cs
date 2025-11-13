using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class ImagingOrder : BaseAuditableEntity
{
    public Guid VisitId { get; set; }
    public PatientVisit Visit { get; set; } = default!;

    public Guid RequestingDoctorId { get; set; }
    public User RequestingDoctor { get; set; } = default!;

    public Modality ModalityRequested { get; set; } = default!;
    public BodyPart BodyPartRequested { get; set; } = default!;
    public string? ReasonForStudy { get; set; }

    public ImagingOrderStatus Status { get; set; }

    // Navigation properties
    public ICollection<DicomStudy> Studies { get; set; } = new List<DicomStudy>();
}
