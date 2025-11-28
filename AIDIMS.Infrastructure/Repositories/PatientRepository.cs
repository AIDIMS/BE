using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Visits)
                .ThenInclude(v => v.AssignedDoctor)
            .Include(p => p.Visits)
                .ThenInclude(v => v.ImagingOrders)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
