using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// AI Analysis Session - Lưu thông tin chung của phiên phân tích
/// </summary>
public class AiAnalysis : BaseAuditableEntity
{
    public Guid StudyId { get; set; }
    public DicomStudy Study { get; set; } = default!;

    public string ModelVersion { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }

    // Tóm tắt kết quả
    public string? PrimaryFinding { get; set; } // Bệnh có điểm cao nhất
    public decimal? OverallConfidence { get; set; } // Điểm của bệnh cao nhất (0-1)

    public bool IsReviewed { get; set; } = false;

    // Navigation property
    public ICollection<AiFinding> Findings { get; set; } = new List<AiFinding>();
}
