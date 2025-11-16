using AIDIMS.Domain.Entities;

namespace AIDIMS.Domain.Interfaces;

public interface IDicomStudyRepository : IRepository<DicomStudy>
{
    Task<DicomStudy?> GetByStudyUidAsync(string studyUid, CancellationToken cancellationToken = default);
    Task<DicomStudy?> GetByOrthancStudyIdAsync(string orthancStudyId, CancellationToken cancellationToken = default);
}

public interface IDicomSeriesRepository : IRepository<DicomSeries>
{
    Task<DicomSeries?> GetBySeriesUidAsync(string seriesUid, CancellationToken cancellationToken = default);
    Task<DicomSeries?> GetByOrthancSeriesIdAsync(string orthancSeriesId, CancellationToken cancellationToken = default);
}

public interface IDicomInstanceRepository : IRepository<DicomInstance>
{
    Task<DicomInstance?> GetBySopInstanceUidAsync(string sopInstanceUid, CancellationToken cancellationToken = default);
    Task<DicomInstance?> GetByOrthancInstanceIdAsync(string orthancInstanceId, CancellationToken cancellationToken = default);
}
