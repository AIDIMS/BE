using System.Text.Json.Serialization;

namespace AIDIMS.Application.DTOs;

public class AiAnalysisRequestDto
{
    [JsonPropertyName("model_version")]
    public string ModelVersion { get; set; } = string.Empty;

    [JsonPropertyName("classification")]
    public ClassificationDto Classification { get; set; } = new();

    [JsonPropertyName("findings")]
    public List<FindingDto> Findings { get; set; } = new();
}

public class ClassificationDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // NORMAL, ABNORMAL

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
}

public class FindingDto
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("confidence_score")]
    public decimal ConfidenceScore { get; set; }

    [JsonPropertyName("bbox_xyxy")]
    public List<decimal> BboxXyxy { get; set; } = new(); // [xmin, ymin, xmax, ymax]
}

public class CreateAiAnalysisDto
{
    public Guid StudyId { get; set; }
    public AiAnalysisRequestDto AnalysisResult { get; set; } = new();
}

public class AiAnalysisResponseDto
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public string? PrimaryFinding { get; set; }
    public decimal? OverallConfidence { get; set; }
    public bool IsReviewed { get; set; }
    public List<AiFindingResponseDto> Findings { get; set; } = new();
}

public class AiFindingResponseDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public decimal? XMin { get; set; }
    public decimal? YMin { get; set; }
    public decimal? XMax { get; set; }
    public decimal? YMax { get; set; }
}

// DTO để request AI analysis cho DICOM instance
public class AnalyzeDicomRequestDto
{
    public Guid StudyId { get; set; }
    public Guid? InstanceId { get; set; } // Optional: specific instance to analyze
}

// DTO để check AI availability cho một study
public class AiAvailabilityDto
{
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public string? BodyPart { get; set; }
    public string? Modality { get; set; }
    public string SupportedCombination { get; set; } = "Chest XRay";
}
