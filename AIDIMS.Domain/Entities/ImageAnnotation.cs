using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Represents annotations on a DICOM instance
/// </summary>
public class ImageAnnotation : BaseAuditableEntity
{
    public Guid InstanceId { get; set; }
    public DicomInstance Instance { get; set; } = default!;

    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;
}
