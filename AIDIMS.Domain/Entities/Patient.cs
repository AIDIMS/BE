using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Represents a patient
/// </summary>
public class Patient : BaseAuditableEntity
{
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    // Navigation properties
    public ICollection<PatientVisit> Visits { get; set; } = new List<PatientVisit>();
    public ICollection<DicomStudy> Studies { get; set; } = new List<DicomStudy>();
}
