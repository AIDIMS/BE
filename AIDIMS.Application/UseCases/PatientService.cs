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
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PatientService(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDetailsDto>.Failure($"Patient with ID {id} not found");
        }

        var patientDto = _mapper.Map<PatientDetailsDto>(patient);

        return Result<PatientDetailsDto>.Success(patientDto);
    }

    public async Task<Result<PagedResult<PatientDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchPatientDto filters,
        CancellationToken cancellationToken = default)
    {
        var patients = await _patientRepository.GetAllAsync(cancellationToken);
        var query = patients.AsEnumerable();

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

        if (!string.IsNullOrWhiteSpace(filters.FullName))
        {
            var name = filters.FullName.Trim().ToLower();
            query = query.Where(p => !string.IsNullOrEmpty(p.FullName)
                                     && p.FullName.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(filters.PhoneNumber))
        {
            var phone = filters.PhoneNumber.Trim();
            query = query.Where(p => !string.IsNullOrEmpty(p.PhoneNumber)
                                     && p.PhoneNumber.Contains(phone));
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
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            var allPatients = await _patientRepository.GetAllAsync(cancellationToken);
            var existingPatient = allPatients.FirstOrDefault(p => 
                !string.IsNullOrEmpty(p.PhoneNumber) && 
                p.PhoneNumber == dto.PhoneNumber.Trim());
            
            if (existingPatient != null)
            {
                return Result<PatientDto>.Failure($"Patient with phone number {dto.PhoneNumber} already exists");
            }
        }

        var patient = _mapper.Map<Patient>(dto);
        
        const int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            patient.PatientCode = await GenerateUniquePatientCodeAsync(cancellationToken);
            
            var allPatientsForCheck = await _patientRepository.GetAllAsync(cancellationToken);
            var isDuplicate = allPatientsForCheck.Any(p => p.PatientCode == patient.PatientCode);
            
            if (!isDuplicate)
            {
                var createdPatient = await _patientRepository.AddAsync(patient, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var patientDto = _mapper.Map<PatientDto>(createdPatient);
                return Result<PatientDto>.Success(patientDto);
            }
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(10, cancellationToken);
            }
        }

        return Result<PatientDto>.Failure("Failed to generate unique patient code after multiple attempts");
    }

    private async Task<string> GenerateUniquePatientCodeAsync(CancellationToken cancellationToken = default)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var ticksPart = (DateTime.UtcNow.Ticks % 1000000).ToString("D6");
        var randomPart = Random.Shared.Next(10, 99);
        
        return await Task.FromResult($"P{datePart}{ticksPart}{randomPart}");
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
