using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class DiagnosisRepository : Repository<Diagnosis>, IDiagnosisRepository
{
    public DiagnosisRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Diagnosis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Study)
                .ThenInclude(s => s.Patient)
            .Include(d => d.Study)
                .ThenInclude(s => s.AssignedDoctor)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public override async Task<IEnumerable<Diagnosis>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Study)
                .ThenInclude(s => s.Patient)
            .Include(d => d.Study)
                .ThenInclude(s => s.AssignedDoctor)
            .Where(d => !d.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Diagnosis?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Study)
                .ThenInclude(s => s.Patient)
            .Include(d => d.Study)
                .ThenInclude(s => s.AssignedDoctor)
            .FirstOrDefaultAsync(d => d.StudyId == studyId && !d.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Diagnosis>> GetByReportStatusAsync(DiagnosisReportStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Study)
                .ThenInclude(s => s.Patient)
            .Include(d => d.Study)
                .ThenInclude(s => s.AssignedDoctor)
            .Where(d => d.ReportStatus == status && !d.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}

