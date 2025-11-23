using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// AI Finding - Lưu từng phát hiện của AI (1-Nhiều với AiAnalysis)
/// </summary>
public class AiFinding : BaseEntity
{
    public Guid AnalysisId { get; set; }
    public AiAnalysis Analysis { get; set; } = default!;

    public string Label { get; set; } = string.Empty; // Tên bệnh (Cardiomegaly, Pneumonia, etc.)
    public decimal ConfidenceScore { get; set; } // Độ tự tin (0-1)

    // Tọa độ Bounding Box (chuẩn XYXY)
    public decimal? XMin { get; set; }
    public decimal? YMin { get; set; }
    public decimal? XMax { get; set; }
    public decimal? YMax { get; set; }
}
