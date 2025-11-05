using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

public class PatientService : IPatientService
{
    private readonly IRepository<Patient> _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PatientService(
        IRepository<Patient> patientRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.Failure($"Patient with ID {id} not found");
        }

        var patientDto = _mapper.Map<PatientDto>(patient);
        return Result<PatientDto>.Success(patientDto);
    }

    public async Task<Result<PagedResult<PatientDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchPatientDto filters,
        CancellationToken cancellationToken = default)
    {
        var patients = await _patientRepository.GetAllAsync(cancellationToken);
        var query = patients.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.FullName))
        {
            var name = filters.FullName.Trim();
            query = query.Where(p => !string.IsNullOrEmpty(p.FullName)
                                     && p.FullName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (filters.DateOfBirth.HasValue)
        {
            var dob = filters.DateOfBirth.Value.Date;
            query = query.Where(p => p.DateOfBirth.Date == dob);
        }

        if (!string.IsNullOrWhiteSpace(filters.Gender))
        {
            var genderStr = filters.Gender.Trim();
            if (Enum.TryParse<Gender>(genderStr, true, out var genderEnum))
            {
                query = query.Where(p => p.Gender == genderEnum);
            }
            else
            {
                query = Enumerable.Empty<Patient>();
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.PhoneNumber))
        {
            var phone = filters.PhoneNumber.Trim();
            query = query.Where(p => !string.IsNullOrEmpty(p.PhoneNumber)
                                     && p.PhoneNumber.IndexOf(phone, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        var totalCount = query.Count();

        var pagedPatients = query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var pagedPatientDtos = _mapper.Map<List<PatientDto>>(pagedPatients);

        var pagedResult = new PagedResult<PatientDto>
        {
            Items = pagedPatientDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<PatientDto>>.Success(pagedResult);
    }

    public async Task<Result<PatientDto>> CreateAsync(CreatePatientDto dto, CancellationToken cancellationToken = default)
    {
        var patient = _mapper.Map<Patient>(dto);
        var createdPatient = await _patientRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var patientDto = _mapper.Map<PatientDto>(createdPatient);
        return Result<PatientDto>.Success(patientDto);
    }

    public async Task<Result<PatientDto>> UpdateAsync(Guid id, UpdatePatientDto dto, CancellationToken cancellationToken = default)
    {
        var existingPatient = await _patientRepository.GetByIdAsync(id, cancellationToken);

        if (existingPatient == null)
        {
            return Result<PatientDto>.Failure($"Patient with ID {id} not found");
        }

        _mapper.Map(dto, existingPatient);
        await _patientRepository.UpdateAsync(existingPatient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var patientDto = _mapper.Map<PatientDto>(existingPatient);
        return Result<PatientDto>.Success(patientDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingPatient = await _patientRepository.GetByIdAsync(id, cancellationToken);

        if (existingPatient == null)
        {
            return Result.Failure($"Patient with ID {id} not found");
        }

        await _patientRepository.DeleteAsync(existingPatient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
