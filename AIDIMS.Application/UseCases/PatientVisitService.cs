using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

public class PatientVisitService : IPatientVisitService
{
    private readonly IRepository<PatientVisit> _patientVisitRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PatientVisitService(
        IRepository<PatientVisit> patientVisitRepository,
        IRepository<Patient> patientRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _patientVisitRepository = patientVisitRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientVisitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visit = await _patientVisitRepository.GetByIdAsync(id, cancellationToken);

        if (visit == null)
        {
            return Result<PatientVisitDto>.Failure($"Patient visit with ID {id} not found");
        }

        // Load related entities
        var patient = await _patientRepository.GetByIdAsync(visit.PatientId, cancellationToken);
        var doctor = await _userRepository.GetByIdAsync(visit.AssignedDoctorId, cancellationToken);

        var visitDto = _mapper.Map<PatientVisitDto>(visit);
        visitDto.PatientName = patient?.FullName ?? "Unknown";
        visitDto.AssignedDoctorName = GetDoctorFullName(doctor);

        return Result<PatientVisitDto>.Success(visitDto);
    }

    public async Task<Result<PagedResult<PatientVisitDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchPatientVisitDto filters,
        CancellationToken cancellationToken = default)
    {
        var visits = await _patientVisitRepository.GetAllAsync(cancellationToken);
        var query = visits.AsEnumerable();

        // Apply filters
        if (filters.PatientId.HasValue)
        {
            query = query.Where(v => v.PatientId == filters.PatientId.Value);
        }

        if (filters.AssignedDoctorId.HasValue)
        {
            query = query.Where(v => v.AssignedDoctorId == filters.AssignedDoctorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var statusStr = filters.Status.Trim();
            if (Enum.TryParse<PatientVisitStatus>(statusStr, true, out var statusEnum))
            {
                query = query.Where(v => v.Status == statusEnum);
            }
            else
            {
                query = Enumerable.Empty<PatientVisit>();
            }
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(v => v.CreatedAt >= filters.FromDate.Value);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(v => v.CreatedAt <= filters.ToDate.Value);
        }

        var totalCount = query.Count();

        var pagedVisits = query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        // Load related entities for all visits
        var patientIds = pagedVisits.Select(v => v.PatientId).Distinct().ToList();
        var doctorIds = pagedVisits.Select(v => v.AssignedDoctorId).Distinct().ToList();

        var patients = new Dictionary<Guid, Patient>();
        var doctors = new Dictionary<Guid, User>();

        foreach (var patientId in patientIds)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient != null)
            {
                patients[patientId] = patient;
            }
        }

        foreach (var doctorId in doctorIds)
        {
            var doctor = await _userRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctor != null)
            {
                doctors[doctorId] = doctor;
            }
        }

        var visitDtos = pagedVisits.Select(v =>
        {
            var dto = _mapper.Map<PatientVisitDto>(v);
            dto.PatientName = patients.TryGetValue(v.PatientId, out var patient) ? patient.FullName : "Unknown";
            dto.AssignedDoctorName = doctors.TryGetValue(v.AssignedDoctorId, out var doc) ? GetDoctorFullName(doc) : "Unknown";
            return dto;
        }).ToList();

        var pagedResult = new PagedResult<PatientVisitDto>
        {
            Items = visitDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<PatientVisitDto>>.Success(pagedResult);
    }

    public async Task<Result<PatientVisitDto>> CreateAsync(CreatePatientVisitDto dto, CancellationToken cancellationToken = default)
    {
        // Validate patient exists
        var patient = await _patientRepository.GetByIdAsync(dto.PatientId, cancellationToken);
        if (patient == null)
        {
            return Result<PatientVisitDto>.Failure($"Patient with ID {dto.PatientId} not found");
        }

        // Validate doctor exists
        var doctor = await _userRepository.GetByIdAsync(dto.AssignedDoctorId, cancellationToken);
        if (doctor == null)
        {
            return Result<PatientVisitDto>.Failure($"Doctor with ID {dto.AssignedDoctorId} not found");
        }

        var visit = _mapper.Map<PatientVisit>(dto);
        var createdVisit = await _patientVisitRepository.AddAsync(visit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var visitDto = _mapper.Map<PatientVisitDto>(createdVisit);
        visitDto.PatientName = patient.FullName;
        visitDto.AssignedDoctorName = GetDoctorFullName(doctor);

        return Result<PatientVisitDto>.Success(visitDto);
    }

    public async Task<Result<PatientVisitDto>> UpdateAsync(Guid id, UpdatePatientVisitDto dto, CancellationToken cancellationToken = default)
    {
        var existingVisit = await _patientVisitRepository.GetByIdAsync(id, cancellationToken);

        if (existingVisit == null)
        {
            return Result<PatientVisitDto>.Failure($"Patient visit with ID {id} not found");
        }

        // Validate doctor exists if changing
        if (dto.AssignedDoctorId.HasValue)
        {
            var newDoctor = await _userRepository.GetByIdAsync(dto.AssignedDoctorId.Value, cancellationToken);
            if (newDoctor == null)
            {
                return Result<PatientVisitDto>.Failure($"Doctor with ID {dto.AssignedDoctorId} not found");
            }
            existingVisit.AssignedDoctorId = dto.AssignedDoctorId.Value;
        }

        // Update symptoms if provided
        if (dto.Symptoms != null)
        {
            existingVisit.Symptoms = dto.Symptoms;
        }

        // Update status if provided
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (Enum.TryParse<PatientVisitStatus>(dto.Status, true, out var statusEnum))
            {
                existingVisit.Status = statusEnum;
            }
            else
            {
                return Result<PatientVisitDto>.Failure($"Invalid status value: {dto.Status}");
            }
        }

        await _patientVisitRepository.UpdateAsync(existingVisit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load related entities
        var patient = await _patientRepository.GetByIdAsync(existingVisit.PatientId, cancellationToken);
        var doctor = await _userRepository.GetByIdAsync(existingVisit.AssignedDoctorId, cancellationToken);

        var visitDto = _mapper.Map<PatientVisitDto>(existingVisit);
        visitDto.PatientName = patient?.FullName ?? "Unknown";
        visitDto.AssignedDoctorName = GetDoctorFullName(doctor);

        return Result<PatientVisitDto>.Success(visitDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingVisit = await _patientVisitRepository.GetByIdAsync(id, cancellationToken);

        if (existingVisit == null)
        {
            return Result.Failure($"Patient visit with ID {id} not found");
        }

        await _patientVisitRepository.DeleteAsync(existingVisit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string GetDoctorFullName(User? user)
    {
        if (user == null) return "Unknown";
        
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
    }
}
