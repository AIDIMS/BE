using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AIDIMS.Application.UseCases;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly IAiAnalysisRepository _analysisRepository;
    private readonly IRepository<DicomStudy> _studyRepository;
    private readonly IRepository<DicomInstance> _instanceRepository;
    private readonly IDicomSeriesRepository _seriesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiAnalysisService> _logger;
    private readonly HttpClient _aiServiceClient;
    private readonly HttpClient _orthancClient;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AiAnalysisService(
        IAiAnalysisRepository analysisRepository,
        IRepository<DicomStudy> studyRepository,
        IRepository<DicomInstance> instanceRepository,
        IDicomSeriesRepository seriesRepository,
        IUnitOfWork unitOfWork,
        ILogger<AiAnalysisService> logger,
        IHttpClientFactory httpClientFactory,
        IDateTimeProvider dateTimeProvider)
    {
        _analysisRepository = analysisRepository;
        _studyRepository = studyRepository;
        _instanceRepository = instanceRepository;
        _seriesRepository = seriesRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _aiServiceClient = httpClientFactory.CreateClient("AiServiceClient");
        _orthancClient = httpClientFactory.CreateClient("OrthancClient");
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AiAnalysisResponseDto> CreateAnalysisAsync(CreateAiAnalysisDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var existingAnalysis = await _analysisRepository.GetByStudyIdAsync(dto.StudyId, cancellationToken);
            if (existingAnalysis != null)
            {
                await _analysisRepository.DeleteAsync(existingAnalysis, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted existing analysis for StudyId: {StudyId}", dto.StudyId);
            }

            var primaryFinding = dto.AnalysisResult.Findings
                .OrderByDescending(f => f.ConfidenceScore)
                .FirstOrDefault();

            var analysis = new AiAnalysis
            {
                StudyId = dto.StudyId,
                ModelVersion = dto.AnalysisResult.ModelVersion,
                AnalysisDate = _dateTimeProvider.Now,
                PrimaryFinding = primaryFinding?.Label,
                OverallConfidence = primaryFinding?.ConfidenceScore,
                IsReviewed = false
            };

            foreach (var findingDto in dto.AnalysisResult.Findings)
            {
                var finding = new AiFinding
                {
                    Label = findingDto.Label,
                    ConfidenceScore = findingDto.ConfidenceScore,
                    XMin = findingDto.BboxXyxy.Count > 0 ? findingDto.BboxXyxy[0] : null,
                    YMin = findingDto.BboxXyxy.Count > 1 ? findingDto.BboxXyxy[1] : null,
                    XMax = findingDto.BboxXyxy.Count > 2 ? findingDto.BboxXyxy[2] : null,
                    YMax = findingDto.BboxXyxy.Count > 3 ? findingDto.BboxXyxy[3] : null
                };

                analysis.Findings.Add(finding);
            }

            await _analysisRepository.AddAsync(analysis, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Created AI Analysis: {AnalysisId} for Study: {StudyId} with {FindingCount} findings",
                analysis.Id, dto.StudyId, analysis.Findings.Count);

            return MapToDto(analysis);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating AI analysis for StudyId: {StudyId}", dto.StudyId);
            throw;
        }
    }

    public async Task<AiAnalysisResponseDto> AnalyzeDicomStudyAsync(Guid studyId, Guid? instanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Kiểm tra điều kiện AI availability
            var availability = await CheckAiAvailabilityAsync(studyId, cancellationToken);
            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException(availability.Reason ?? "AI analysis is not available for this study");
            }

            // 2. Lấy thông tin Study
            var study = await _studyRepository.GetByIdAsync(studyId, cancellationToken);
            if (study == null)
            {
                throw new ArgumentException($"Study with ID {studyId} not found");
            }

            // 3. Lấy instance để gửi cho AI
            DicomInstance? instance = null;
            if (instanceId.HasValue)
            {
                instance = await _instanceRepository.GetByIdAsync(instanceId.Value, cancellationToken);
            }
            else
            {
                // Lấy instance đầu tiên của study
                var instances = await _instanceRepository.GetAllAsync(cancellationToken);
                instance = instances.FirstOrDefault(i => i.Series.StudyId == studyId);
            }

            if (instance == null)
            {
                throw new ArgumentException("No DICOM instance found for this study");
            }

            _logger.LogInformation("Sending DICOM instance {InstanceId} to AI service for analysis", instance.Id);

            _logger.LogInformation("Retrieving image from Orthanc for instance: {OrthancInstanceId}", instance.OrthancInstanceId);
            // 4. Lấy ảnh từ Orthanc
            var imageResponse = await _orthancClient.GetAsync(
                $"/instances/{instance.OrthancInstanceId}/preview",
                cancellationToken);

            if (!imageResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get image from Orthanc: {imageResponse.StatusCode}");
            }

            var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync(cancellationToken);
            _logger.LogInformation("Retrieved image from Orthanc, size: {Size} bytes", imageBytes.Length);

            // 4. Gửi ảnh đến AI service qua form-data
            using var formContent = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formContent.Add(imageContent, "image", $"{instance.OrthancInstanceId}.png");

            _logger.LogInformation("Sending POST request to AI service: /predict_findings");
            var response = await _aiServiceClient.PostAsync("/predict_findings", formContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI service returned error: {StatusCode}, {Content}", response.StatusCode, errorContent);
                throw new Exception($"AI service error: {response.StatusCode} - {errorContent}");
            }

            // 5. Parse kết quả từ AI service
            var aiResult = await response.Content.ReadFromJsonAsync<AiAnalysisRequestDto>(cancellationToken);
            if (aiResult == null)
            {
                throw new Exception("Failed to parse AI service response");
            }

            _logger.LogInformation("Received AI analysis result with {FindingCount} findings", aiResult.Findings.Count);

            // 5. Lưu kết quả vào database
            var createDto = new CreateAiAnalysisDto
            {
                StudyId = studyId,
                AnalysisResult = aiResult
            };

            return await CreateAnalysisAsync(createDto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing DICOM study {StudyId}", studyId);
            throw;
        }
    }

    public async Task<AiAnalysisResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var analysis = await _analysisRepository.GetWithFindingsAsync(id, cancellationToken);
        return analysis == null ? null : MapToDto(analysis);
    }

    public async Task<AiAnalysisResponseDto?> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        var analysis = await _analysisRepository.GetByStudyIdAsync(studyId, cancellationToken);
        return analysis == null ? null : MapToDto(analysis);
    }

    public async Task<AiAnalysisResponseDto?> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        // Tìm study từ instance ID
        var instance = await _instanceRepository.GetByIdAsync(instanceId, cancellationToken);
        if (instance == null)
        {
            return null;
        }

        // Lấy series để tìm study
        var series = await _seriesRepository.GetByIdAsync(instance.SeriesId, cancellationToken);
        if (series == null)
        {
            return null;
        }

        var analysis = await _analysisRepository.GetByStudyIdAsync(series.StudyId, cancellationToken);
        return analysis == null ? null : MapToDto(analysis);
    }

    public async Task<AiAnalysisResponseDto?> GetByOrthancInstanceIdAsync(string orthancInstanceId, CancellationToken cancellationToken = default)
    {
        // Tìm instance từ OrthancInstanceId
        var instances = await _instanceRepository.GetAllAsync(cancellationToken);
        var instance = instances.FirstOrDefault(i => i.OrthancInstanceId == orthancInstanceId);

        if (instance == null)
        {
            return null;
        }

        // Lấy series để tìm study
        var series = await _seriesRepository.GetByIdAsync(instance.SeriesId, cancellationToken);
        if (series == null)
        {
            return null;
        }

        var analysis = await _analysisRepository.GetByStudyIdAsync(series.StudyId, cancellationToken);
        return analysis == null ? null : MapToDto(analysis);
    }

    public async Task<bool> MarkAsReviewedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var analysis = await _analysisRepository.GetByIdAsync(id, cancellationToken);
        if (analysis == null)
        {
            return false;
        }

        analysis.IsReviewed = true;
        await _analysisRepository.UpdateAsync(analysis, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked analysis {AnalysisId} as reviewed", id);
        return true;
    }

    public async Task<AiAvailabilityDto> CheckAiAvailabilityAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        var study = await _studyRepository.GetByIdAsync(studyId, cancellationToken);
        if (study == null)
        {
            return new AiAvailabilityDto
            {
                IsAvailable = false,
                Reason = "Study not found"
            };
        }

        var isAvailable = study.Order != null
            && study.Order.BodyPartRequested == BodyPart.Chest
            && study.Modality == Modality.XRay;

        if (isAvailable)
        {
            return new AiAvailabilityDto
            {
                IsAvailable = true,
                BodyPart = study.Order.BodyPartRequested.ToString(),
                Modality = study.Modality.ToString(),
                SupportedCombination = "Chest XRay"
            };
        }
        else
        {
            var bodyPart = study.Order?.BodyPartRequested.ToString() ?? "Unknown";
            var modality = study.Modality.ToString();
            var reason = $"AI analysis is only available for Chest XRay studies. Current study: {bodyPart} {modality}";

            return new AiAvailabilityDto
            {
                IsAvailable = false,
                Reason = reason,
                BodyPart = bodyPart,
                Modality = modality,
                SupportedCombination = "Chest XRay"
            };
        }
    }

    private static AiAnalysisResponseDto MapToDto(AiAnalysis analysis)
    {
        return new AiAnalysisResponseDto
        {
            Id = analysis.Id,
            StudyId = analysis.StudyId,
            ModelVersion = analysis.ModelVersion,
            AnalysisDate = analysis.AnalysisDate,
            PrimaryFinding = analysis.PrimaryFinding,
            OverallConfidence = analysis.OverallConfidence,
            IsReviewed = analysis.IsReviewed,
            Findings = analysis.Findings.Select(f => new AiFindingResponseDto
            {
                Id = f.Id,
                Label = f.Label,
                ConfidenceScore = f.ConfidenceScore,
                XMin = f.XMin,
                YMin = f.YMin,
                XMax = f.XMax,
                YMax = f.YMax
            }).ToList()
        };
    }
}
