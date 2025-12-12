using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class PatientVisit : BaseAuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid AssignedDoctorId { get; set; }
    public User AssignedDoctor { get; set; } = default!;

    public string? Symptoms { get; set; }
    public PatientVisitStatus Status { get; set; } = PatientVisitStatus.Waiting;

    // Navigation properties
    public ICollection<ImagingOrder> ImagingOrders { get; set; } = new List<ImagingOrder>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
