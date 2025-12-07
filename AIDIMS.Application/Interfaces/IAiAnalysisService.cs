using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IAiAnalysisService
{
    Task<AiAnalysisResponseDto> CreateAnalysisAsync(CreateAiAnalysisDto dto, CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto> AnalyzeDicomStudyAsync(Guid studyId, Guid? instanceId, CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto?> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto?> GetByOrthancInstanceIdAsync(string orthancInstanceId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReviewedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiAvailabilityDto> CheckAiAvailabilityAsync(Guid studyId, CancellationToken cancellationToken = default);
}
