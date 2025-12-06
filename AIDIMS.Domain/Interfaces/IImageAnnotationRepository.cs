using AIDIMS.Domain.Entities;

namespace AIDIMS.Domain.Interfaces;

public interface IImageAnnotationRepository : IRepository<ImageAnnotation>
{
    Task<IEnumerable<ImageAnnotation>> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImageAnnotation>> GetByAnnotationTypeAsync(string annotationType, CancellationToken cancellationToken = default);
}

