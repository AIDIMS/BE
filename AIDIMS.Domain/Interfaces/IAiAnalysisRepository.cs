using AIDIMS.Domain.Entities;

namespace AIDIMS.Domain.Interfaces;

public interface IAiAnalysisRepository : IRepository<AiAnalysis>
{
    Task<AiAnalysis?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task<AiAnalysis?> GetWithFindingsAsync(Guid id, CancellationToken cancellationToken = default);
}
