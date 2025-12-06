using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class ImageAnnotationRepository : Repository<ImageAnnotation>, IImageAnnotationRepository
{
    public ImageAnnotationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<ImageAnnotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ia => ia.Instance)
                .ThenInclude(di => di.Series)
                    .ThenInclude(ds => ds.Study)
            .FirstOrDefaultAsync(ia => ia.Id == id && !ia.IsDeleted, cancellationToken);
    }

    public override async Task<IEnumerable<ImageAnnotation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ia => ia.Instance)
            .Where(ia => !ia.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ImageAnnotation>> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ia => ia.Instance)
            .Where(ia => ia.InstanceId == instanceId && !ia.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ImageAnnotation>> GetByAnnotationTypeAsync(string annotationType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ia => ia.Instance)
            .Where(ia => ia.AnnotationType == annotationType && !ia.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}

