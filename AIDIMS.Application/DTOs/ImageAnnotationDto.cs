using System.Text.Json.Serialization;

namespace AIDIMS.Application.DTOs;

public class ImageAnnotationDto
{
    public Guid Id { get; set; }
    public Guid InstanceId { get; set; }
    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ParsedAnnotationData { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? InstanceSopInstanceUid { get; set; }
}

public class CreateImageAnnotationDto
{
    public string InstanceId { get; set; } = string.Empty;
    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;
}

public class UpdateImageAnnotationDto
{
    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;
}

public class SearchImageAnnotationDto
{
    public Guid? InstanceId { get; set; }
    public string? AnnotationType { get; set; }
}

public class BoundingBoxAnnotationData
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("box")]
    public List<int> Box { get; set; } = new();
}
