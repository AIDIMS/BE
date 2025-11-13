using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

public class ImageAnnotation : BaseAuditableEntity
{
    public Guid InstanceId { get; set; }
    public DicomInstance Instance { get; set; } = default!;

    public string AnnotationType { get; set; } = string.Empty;
    public string AnnotationData { get; set; } = string.Empty;
}
