using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class ImagingOrderRepository : Repository<ImagingOrder>, IImagingOrderRepository
{
    public ImagingOrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ImagingOrder>> GetAllWithStudiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ImagingOrders
            .Include(o => o.Studies)
            .Where(o => !o.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
