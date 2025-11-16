using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDIMS.Infrastructure.Repositories;

public class DicomStudyRepository : Repository<DicomStudy>, IDicomStudyRepository
{
    public DicomStudyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DicomStudy?> GetByStudyUidAsync(string studyUid, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.StudyUid == studyUid && !s.IsDeleted, cancellationToken);
    }

    public async Task<DicomStudy?> GetByOrthancStudyIdAsync(string orthancStudyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.OrthancStudyId == orthancStudyId && !s.IsDeleted, cancellationToken);
    }
}

public class DicomSeriesRepository : Repository<DicomSeries>, IDicomSeriesRepository
{
    public DicomSeriesRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DicomSeries?> GetBySeriesUidAsync(string seriesUid, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.SeriesUid == seriesUid && !s.IsDeleted, cancellationToken);
    }

    public async Task<DicomSeries?> GetByOrthancSeriesIdAsync(string orthancSeriesId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.OrthancSeriesId == orthancSeriesId && !s.IsDeleted, cancellationToken);
    }
}

public class DicomInstanceRepository : Repository<DicomInstance>, IDicomInstanceRepository
{
    public DicomInstanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DicomInstance?> GetBySopInstanceUidAsync(string sopInstanceUid, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.SopInstanceUid == sopInstanceUid && !i.IsDeleted, cancellationToken);
    }

    public async Task<DicomInstance?> GetByOrthancInstanceIdAsync(string orthancInstanceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.OrthancInstanceId == orthancInstanceId && !i.IsDeleted, cancellationToken);
    }
}
