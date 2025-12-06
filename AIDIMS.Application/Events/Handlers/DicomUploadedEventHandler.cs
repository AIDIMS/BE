using AIDIMS.Application.Interfaces;
using AIDIMS.Application.Events;
using AIDIMS.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AIDIMS.Application.Events.Handlers;

/// <summary>
/// Handler xử lý event DicomUploadedEvent - tự động gọi AI analysis
/// </summary>
public class DicomUploadedEventHandler : IEventHandler<DicomUploadedEvent>
{
    private readonly IAiAnalysisService _aiAnalysisService;
    private readonly ILogger<DicomUploadedEventHandler> _logger;

    public DicomUploadedEventHandler(
        IAiAnalysisService aiAnalysisService,
        ILogger<DicomUploadedEventHandler> logger)
    {
        _aiAnalysisService = aiAnalysisService;
        _logger = logger;
    }

    public async Task HandleAsync(DicomUploadedEvent eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing DicomUploadedEvent - StudyId: {StudyId}, InstanceId: {InstanceId}",
                eventData.StudyId, eventData.InstanceId);

            await _aiAnalysisService.AnalyzeDicomStudyAsync(
                eventData.StudyId,
                eventData.InstanceId,
                cancellationToken);

            _logger.LogInformation(
                "AI analysis completed successfully for StudyId: {StudyId}",
                eventData.StudyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process DicomUploadedEvent for StudyId: {StudyId}. Error: {ErrorMessage}",
                eventData.StudyId,
                ex.Message);
        }
    }
}

