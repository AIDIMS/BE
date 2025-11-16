using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

public class DicomInstance : BaseAuditableEntity
{
    public Guid SeriesId { get; set; }
    public DicomSeries Series { get; set; } = default!;

    public string OrthancInstanceId { get; set; } = string.Empty;
    public string SopInstanceUid { get; set; } = string.Empty;
    public int? InstanceNumber { get; set; }
    public string ImagePath { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<ImageAnnotation> Annotations { get; set; } = new List<ImageAnnotation>();
}
