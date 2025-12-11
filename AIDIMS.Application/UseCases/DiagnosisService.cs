using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

public class DiagnosisService : IDiagnosisService
{
    private readonly IDiagnosisRepository _diagnosisRepository;
    private readonly IDicomStudyRepository _studyRepository;
    private readonly IImagingOrderRepository _orderRepository;
    private readonly IPatientVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DiagnosisService(
        IDiagnosisRepository diagnosisRepository,
        IDicomStudyRepository studyRepository,
        IImagingOrderRepository orderRepository,
        IPatientVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _diagnosisRepository = diagnosisRepository;
        _studyRepository = studyRepository;
        _orderRepository = orderRepository;
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DiagnosisDto>> CreateAsync(CreateDiagnosisDto dto, CancellationToken cancellationToken = default)
    {
        // Validate that the study exists
        var study = await _studyRepository.GetByIdAsync(dto.StudyId, cancellationToken);
        if (study == null)
        {
            return Result<DiagnosisDto>.Failure($"DICOM Study with ID {dto.StudyId} not found");
        }

        // Check if diagnosis already exists for this study
        var existingDiagnosis = await _diagnosisRepository.GetByStudyIdAsync(dto.StudyId, cancellationToken);
        if (existingDiagnosis != null)
        {
            return Result<DiagnosisDto>.Failure($"Diagnosis already exists for Study with ID {dto.StudyId}. Use update instead.");
        }

        var diagnosis = _mapper.Map<Diagnosis>(dto);
        var createdDiagnosis = await _diagnosisRepository.AddAsync(diagnosis, cancellationToken);

        // Update PatientVisit status to Done
        // Load the Order to get VisitId
        var order = await _orderRepository.GetByIdAsync(study.OrderId, cancellationToken);
        if (order != null)
        {
            var visit = await _visitRepository.GetByIdAsync(order.VisitId, cancellationToken);
            if (visit != null)
            {
                visit.Status = PatientVisitStatus.Done;
                await _visitRepository.UpdateAsync(visit, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var diagnosisDto = _mapper.Map<DiagnosisDto>(createdDiagnosis);
        return Result<DiagnosisDto>.Success(diagnosisDto);
    }

    public async Task<Result<DiagnosisDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var diagnosis = await _diagnosisRepository.GetByIdAsync(id, cancellationToken);

        if (diagnosis == null)
        {
            return Result<DiagnosisDto>.Failure($"Diagnosis with ID {id} not found");
        }

        var diagnosisDto = _mapper.Map<DiagnosisDto>(diagnosis);
        return Result<DiagnosisDto>.Success(diagnosisDto);
    }

    public async Task<Result<DiagnosisDto>> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        var diagnosis = await _diagnosisRepository.GetByStudyIdAsync(studyId, cancellationToken);

        if (diagnosis == null)
        {
            return Result<DiagnosisDto>.Failure($"Diagnosis not found for Study with ID {studyId}");
        }

        var diagnosisDto = _mapper.Map<DiagnosisDto>(diagnosis);
        return Result<DiagnosisDto>.Success(diagnosisDto);
    }

    public async Task<Result<PagedResult<DiagnosisDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchDiagnosisDto filters,
        CancellationToken cancellationToken = default)
    {
        var diagnoses = await _diagnosisRepository.GetAllAsync(cancellationToken);
        var query = diagnoses.AsEnumerable();

        if (filters.StudyId.HasValue)
        {
            query = query.Where(d => d.StudyId == filters.StudyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.ReportStatus))
        {
            if (Enum.TryParse<DiagnosisReportStatus>(filters.ReportStatus, true, out var status))
            {
                query = query.Where(d => d.ReportStatus == status);
            }
            else
            {
                query = Enumerable.Empty<Diagnosis>();
            }
        }

        if (filters.PatientId.HasValue)
        {
            query = query.Where(d => d.Study != null && d.Study.PatientId == filters.PatientId.Value);
        }

        if (filters.DoctorId.HasValue)
        {
            query = query.Where(d => d.Study != null && d.Study.AssignedDoctorId == filters.DoctorId.Value);
        }

        var totalCount = query.Count();

        var pagedDiagnoses = query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var pagedDiagnosisDtos = _mapper.Map<List<DiagnosisDto>>(pagedDiagnoses);

        var pagedResult = new PagedResult<DiagnosisDto>
        {
            Items = pagedDiagnosisDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<DiagnosisDto>>.Success(pagedResult);
    }

    public async Task<Result<DiagnosisDto>> UpdateAsync(Guid id, UpdateDiagnosisDto dto, CancellationToken cancellationToken = default)
    {
        var existingDiagnosis = await _diagnosisRepository.GetByIdAsync(id, cancellationToken);

        if (existingDiagnosis == null)
        {
            return Result<DiagnosisDto>.Failure($"Diagnosis with ID {id} not found");
        }

        _mapper.Map(dto, existingDiagnosis);
        await _diagnosisRepository.UpdateAsync(existingDiagnosis, cancellationToken);

        // Update PatientVisit status to Done if not already done
        var study = await _studyRepository.GetByIdAsync(existingDiagnosis.StudyId, cancellationToken);
        if (study != null)
        {
            var order = await _orderRepository.GetByIdAsync(study.OrderId, cancellationToken);
            if (order != null)
            {
                var visit = await _visitRepository.GetByIdAsync(order.VisitId, cancellationToken);
                if (visit != null && visit.Status != PatientVisitStatus.Done)
                {
                    visit.Status = PatientVisitStatus.Done;
                    await _visitRepository.UpdateAsync(visit, cancellationToken);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var diagnosisDto = _mapper.Map<DiagnosisDto>(existingDiagnosis);
        return Result<DiagnosisDto>.Success(diagnosisDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingDiagnosis = await _diagnosisRepository.GetByIdAsync(id, cancellationToken);

        if (existingDiagnosis == null)
        {
            return Result.Failure($"Diagnosis with ID {id} not found");
        }

        await _diagnosisRepository.DeleteAsync(existingDiagnosis, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

