using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Represents AI analysis results for a DICOM study
/// </summary>
public class AiResult : BaseAuditableEntity
{
    public Guid StudyId { get; set; }
    public DicomStudy Study { get; set; } = default!;

    public string ModelVersion { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public string Classification { get; set; } = string.Empty;
    public decimal? ConfidenceScore { get; set; }
    public string? DetailedOutput { get; set; }
    public bool IsReviewed { get; set; } = false;
}
