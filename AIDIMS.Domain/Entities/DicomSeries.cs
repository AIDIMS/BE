using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class DicomSeries : BaseAuditableEntity
{
    public Guid StudyId { get; set; }
    public DicomStudy Study { get; set; } = default!;

    public string OrthancSeriesId { get; set; } = string.Empty;
    public string SeriesUid { get; set; } = string.Empty;
    public string? SeriesDescription { get; set; }
    public int? SeriesNumber { get; set; }
    public Modality Modality { get; set; } = default!;

    // Navigation properties
    public ICollection<DicomInstance> Instances { get; set; } = new List<DicomInstance>();
}
