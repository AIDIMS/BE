using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = default!;
    public Department Department { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    // Navigation properties
    public ICollection<PatientVisit> AssignedVisits { get; set; } = new List<PatientVisit>();
    public ICollection<ImagingOrder> RequestedOrders { get; set; } = new List<ImagingOrder>();
    public ICollection<DicomStudy> AssignedStudies { get; set; } = new List<DicomStudy>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
