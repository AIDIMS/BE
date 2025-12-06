using AIDIMS.Domain.Entities;

namespace AIDIMS.Domain.Interfaces;

public interface IDiagnosisRepository : IRepository<Diagnosis>
{
    Task<Diagnosis?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Diagnosis>> GetByReportStatusAsync(Domain.Enums.DiagnosisReportStatus status, CancellationToken cancellationToken = default);
}

