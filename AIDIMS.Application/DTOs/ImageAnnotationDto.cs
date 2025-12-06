namespace AIDIMS.Application.DTOs;

public class ImageAnnotationDto
{
    public Guid Id { get; set; }
    public Guid InstanceId { get; set; }
    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? InstanceSopInstanceUid { get; set; }
}

public class CreateImageAnnotationDto
{
    public Guid InstanceId { get; set; }
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

