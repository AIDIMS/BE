using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class AiAnalysisRepository : Repository<AiAnalysis>, IAiAnalysisRepository
{
    public AiAnalysisRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AiAnalysis?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Findings)
            .FirstOrDefaultAsync(a => a.StudyId == studyId, cancellationToken);
    }

    public async Task<AiAnalysis?> GetWithFindingsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Findings)
            .Include(a => a.Study)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
