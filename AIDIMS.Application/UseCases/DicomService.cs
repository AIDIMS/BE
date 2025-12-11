using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Application.Events;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Events;
using AIDIMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIDIMS.Application.UseCases;

public class DicomService : IDicomService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<DicomService> _logger;
    private readonly IDicomStudyRepository _studyRepository;
    private readonly IDicomSeriesRepository _seriesRepository;
    private readonly IDicomInstanceRepository _instanceRepository;
    private readonly IRepository<ImagingOrder> _orderRepository;
    private readonly IRepository<PatientVisit> _visitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DicomService(
        IHttpClientFactory httpClientFactory,
        ILogger<DicomService> logger,
        IDicomStudyRepository studyRepository,
        IDicomSeriesRepository seriesRepository,
        IDicomInstanceRepository instanceRepository,
        IRepository<ImagingOrder> orderRepository,
        IRepository<PatientVisit> visitRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IDateTimeProvider dateTimeProvider)
    {
        _httpClient = httpClientFactory.CreateClient("OrthancClient");
        _logger = logger;
        _studyRepository = studyRepository;
        _seriesRepository = seriesRepository;
        _instanceRepository = instanceRepository;
        _orderRepository = orderRepository;
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _dateTimeProvider = dateTimeProvider;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<DicomUploadResultDto?> UploadInstanceAsync(DicomUploadDto dicom, CancellationToken cancellationToken = default)
    {
        if (dicom.File.Length == 0)
        {
            throw new ArgumentException("The provided DICOM file is empty.");
        }

        // Upload to Orthanc
        using var content = new StreamContent(dicom.File.OpenReadStream());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/dicom");

        var response = await _httpClient.PostAsync("/instances", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Failed to upload DICOM instance. Status Code: {response.StatusCode}, Body: {errorBody}");
        }

        DicomUploadResultDto? uploadResult;
        try
        {
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Orthanc Response JSON: {JsonResponse}", jsonString);
            uploadResult = JsonSerializer.Deserialize<DicomUploadResultDto>(jsonString, _jsonOptions);

            if (uploadResult == null)
            {
                return null;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Orthanc response");
            return null;
        }

        //Get metadata from Orthanc
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Get instance metadata
            var instanceMetadata = await GetInstanceMetadataAsync(uploadResult.ID, cancellationToken);
            if (instanceMetadata == null)
            {
                _logger.LogWarning("Failed to get instance metadata from Orthanc");
                return uploadResult;
            }

            // Get series metadata
            var seriesMetadata = await GetSeriesMetadataAsync(uploadResult.ParentSeries, cancellationToken);
            if (seriesMetadata == null)
            {
                _logger.LogWarning("Failed to get series metadata from Orthanc");
                return uploadResult;
            }

            // Get study metadata
            var studyMetadata = await GetStudyMetadataAsync(uploadResult.ParentStudy, cancellationToken);
            if (studyMetadata == null)
            {
                _logger.LogWarning("Failed to get study metadata from Orthanc");
                return uploadResult;
            }

            // Save to database
            var (studyId, instanceId, order) = await SaveDicomDataAsync(
                studyMetadata,
                seriesMetadata,
                instanceMetadata,
                uploadResult,
                dicom,
                cancellationToken);

            // Update ImagingOrder status to Completed
            if (order.Status != ImagingOrderStatus.Completed)
            {
                order.Status = ImagingOrderStatus.Completed;
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Updated ImagingOrder {OrderId} status to Completed", order.Id);
            }

            // Update PatientVisit status back to Waiting
            var visit = await _visitRepository.GetByIdAsync(order.VisitId, cancellationToken);
            if (visit != null && visit.Status != PatientVisitStatus.Waiting)
            {
                visit.Status = PatientVisitStatus.Waiting;
                await _visitRepository.UpdateAsync(visit, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Updated PatientVisit {VisitId} status to Waiting", visit.Id);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully saved DICOM data to database. StudyId: {StudyId}, InstanceId: {InstanceId}", studyId, instanceId);

            // Publish event trigger AI analysis - chỉ khi BodyPart là Chest và Modality là XRay
            if (studyId != Guid.Empty && instanceId != Guid.Empty)
            {

                var study = await _studyRepository.GetByIdAsync(studyId, cancellationToken);
                if (study == null)
                {
                    _logger.LogWarning("Cannot publish DicomUploadedEvent: Study not found. StudyId: {StudyId}", studyId);
                }
                else
                {
                    var shouldTriggerAI = study.Order != null
                        && study.Order.BodyPartRequested == BodyPart.Chest
                        && study.Modality == Modality.XRay;

                    if (shouldTriggerAI)
                    {
                        var dicomUploadedEvent = new DicomUploadedEvent
                        {
                            StudyId = studyId,
                            InstanceId = instanceId,
                            UploadedAt = _dateTimeProvider.Now
                        };

                        // Publish event - không await để không block response
                        _logger.LogInformation(
                            "Publishing DicomUploadedEvent for StudyId: {StudyId}, InstanceId: {InstanceId}. BodyPart: {BodyPart}, Modality: {Modality}",
                            studyId, instanceId, study.Order!.BodyPartRequested, study.Modality);

                        _ = _eventPublisher.PublishAsync(dicomUploadedEvent, cancellationToken)
                            .ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    _logger.LogError(task.Exception?.GetBaseException(),
                                        "Failed to publish DicomUploadedEvent for StudyId: {StudyId}. Error: {ErrorMessage}",
                                        studyId, task.Exception?.GetBaseException()?.Message);
                                }
                                else if (task.IsCompletedSuccessfully)
                                {
                                    _logger.LogInformation("DicomUploadedEvent published successfully for StudyId: {StudyId}. AI analysis will be triggered.", studyId);
                                }
                            }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Skipping AI analysis trigger for StudyId: {StudyId}. BodyPart: {BodyPart}, Modality: {Modality}. AI analysis only supports Chest XRay studies.",
                            studyId,
                            study.Order?.BodyPartRequested ?? BodyPart.Other,
                            study.Modality);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Cannot publish DicomUploadedEvent: StudyId or InstanceId is empty. StudyId: {StudyId}, InstanceId: {InstanceId}", studyId, instanceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save DICOM data to database");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        }

        return uploadResult;
    }

    private async Task<OrthancInstanceDto?> GetInstanceMetadataAsync(string instanceId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/instances/{instanceId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<OrthancInstanceDto>(jsonString, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get instance metadata from Orthanc");
            return null;
        }
    }

    private async Task<OrthancSeriesDto?> GetSeriesMetadataAsync(string seriesId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/series/{seriesId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<OrthancSeriesDto>(jsonString, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get series metadata from Orthanc");
            return null;
        }
    }

    private async Task<OrthancStudyDto?> GetStudyMetadataAsync(string studyId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/studies/{studyId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<OrthancStudyDto>(jsonString, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get study metadata from Orthanc");
            return null;
        }
    }

    private async Task<(Guid StudyId, Guid InstanceId, ImagingOrder Order)> SaveDicomDataAsync(
        OrthancStudyDto studyMetadata,
        OrthancSeriesDto seriesMetadata,
        OrthancInstanceDto instanceMetadata,
        DicomUploadResultDto uploadResult,
        DicomUploadDto dicom,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(dicom.OrderId!.Value, cancellationToken);
        if (order == null)
        {
            throw new ArgumentException($"ImagingOrder with ID {dicom.OrderId} not found");
        }

        // Get or create Study
        var studyUid = studyMetadata.MainDicomTags.GetValueOrDefault("StudyInstanceUID", string.Empty);
        var study = await _studyRepository.GetByStudyUidAsync(studyUid, cancellationToken);

        if (study == null)
        {
            // Parse study date
            var studyDateStr = studyMetadata.MainDicomTags.GetValueOrDefault("StudyDate", string.Empty);
            DateTime studyDate = _dateTimeProvider.Now;
            if (!string.IsNullOrEmpty(studyDateStr) && studyDateStr.Length >= 8)
            {
                if (DateTime.TryParseExact(studyDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    studyDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
                }
            }

            var modalityStr = studyMetadata.MainDicomTags.GetValueOrDefault("Modality", string.Empty);
            var modality = ParseModality(modalityStr);

            if (!dicom.OrderId.HasValue || !dicom.PatientId.HasValue)
            {
                throw new ArgumentException("OrderId and PatientId are required to create a new study");
            }

            _logger.LogInformation("Creating new study: StudyUID={StudyUid}, PatientId={PatientId}, OrderId={OrderId}",
                studyUid, dicom.PatientId, dicom.OrderId);

            study = new DicomStudy
            {
                OrthancStudyId = uploadResult.ParentStudy,
                StudyUid = studyUid,
                StudyDescription = studyMetadata.MainDicomTags.GetValueOrDefault("StudyDescription"),
                AccessionNumber = studyMetadata.MainDicomTags.GetValueOrDefault("AccessionNumber"),
                Modality = modality,
                StudyDate = studyDate,
                OrderId = dicom.OrderId.Value,
                PatientId = dicom.PatientId.Value,
                AssignedDoctorId = order.RequestingDoctorId
            };

            await _studyRepository.AddAsync(study, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Study created successfully: Id={StudyId}", study.Id);
        }
        else
        {
            _logger.LogInformation("Study already exists: Id={StudyId}, StudyUid={StudyUid}", study.Id, studyUid);
        }

        // Get or create Series
        var seriesUid = seriesMetadata.MainDicomTags.GetValueOrDefault("SeriesInstanceUID", string.Empty);
        var series = await _seriesRepository.GetBySeriesUidAsync(seriesUid, cancellationToken);

        if (series == null)
        {
            var modalityStr = seriesMetadata.MainDicomTags.GetValueOrDefault("Modality", string.Empty);
            var modality = ParseModality(modalityStr);

            int? seriesNumber = null;
            if (seriesMetadata.MainDicomTags.TryGetValue("SeriesNumber", out var seriesNumStr))
            {
                if (int.TryParse(seriesNumStr, out var parsed))
                {
                    seriesNumber = parsed;
                }
            }

            series = new DicomSeries
            {
                StudyId = study.Id,
                OrthancSeriesId = uploadResult.ParentSeries,
                SeriesUid = seriesUid,
                SeriesDescription = seriesMetadata.MainDicomTags.GetValueOrDefault("SeriesDescription"),
                SeriesNumber = seriesNumber,
                Modality = modality
            };

            await _seriesRepository.AddAsync(series, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Series created successfully: Id={SeriesId}", series.Id);
        }
        else
        {
            _logger.LogInformation("Series already exists: Id={SeriesId}, SeriesUid={SeriesUid}", series.Id, seriesUid);
        }

        // Get or create Instance
        var sopInstanceUid = instanceMetadata.MainDicomTags.GetValueOrDefault("SOPInstanceUID", string.Empty);
        var instance = await _instanceRepository.GetBySopInstanceUidAsync(sopInstanceUid, cancellationToken);

        if (instance == null)
        {
            int? instanceNumber = null;
            if (instanceMetadata.MainDicomTags.TryGetValue("InstanceNumber", out var instanceNumStr))
            {
                if (int.TryParse(instanceNumStr, out var parsed))
                {
                    instanceNumber = parsed;
                }
            }

            instance = new DicomInstance
            {
                SeriesId = series.Id,
                OrthancInstanceId = uploadResult.ID,
                SopInstanceUid = sopInstanceUid,
                InstanceNumber = instanceNumber,
                ImagePath = uploadResult.Path
            };

            await _instanceRepository.AddAsync(instance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Instance created successfully: Id={InstanceId}", instance.Id);
        }
        else
        {
            _logger.LogInformation("Instance already exists: Id={InstanceId}, SopInstanceUid={SopInstanceUid}", instance.Id, sopInstanceUid);
        }

        // Return studyId, instanceId and order for status update
        return (study.Id, instance.Id, order);
    }

    private Modality ParseModality(string modalityStr)
    {
        return modalityStr?.ToUpperInvariant() switch
        {
            "CR" or "DX" or "XR" => Modality.XRay,
            "CT" => Modality.CTScan,
            "MR" => Modality.MRI,
            "US" => Modality.Ultrasound,
            "MG" => Modality.Mammography,
            "XA" or "RF" => Modality.Fluoroscopy,
            "NM" or "PT" => Modality.NuclearMedicine,
            _ => Modality.XRay
        };
    }

    public async Task<IEnumerable<DicomInstanceDto>> GetInstancesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var instances = await _instanceRepository.GetByOrderIdAsync(orderId, cancellationToken);

        return instances.Select(instance => new DicomInstanceDto
        {
            Id = instance.Id,
            InstanceId = instance.OrthancInstanceId,
            StudyId = instance.Series.StudyId,
            SeriesId = instance.SeriesId,
            Filename = instance.ImagePath.Split('\\').Last() ?? $"{instance.SopInstanceUid}.dcm",
            UploadedAt = instance.CreatedAt,
            Modality = instance.Series.Study.Modality.ToString(),
            BodyPart = instance.Series.Study.Order?.BodyPartRequested.ToString() ?? "Unknown"
        });
    }

    public async Task<byte[]?> DownloadInstanceAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/instances/{instanceId}/file", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading instance {InstanceId} from Orthanc", instanceId);
            return null;
        }
    }

    public async Task<byte[]?> GetInstancePreviewAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/instances/{instanceId}/preview", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get preview for instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview for instance {InstanceId} from Orthanc", instanceId);
            return null;
        }
    }
}
