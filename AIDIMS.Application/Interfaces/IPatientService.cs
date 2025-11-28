using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IPatientService
{
    Task<Result<PatientDto>> CreateAsync(CreatePatientDto dto, CancellationToken cancellationToken = default);
    Task<Result<PatientDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PatientDto>>> GetAllAsync(PaginationParams paginationParams, SearchPatientDto filters, CancellationToken cancellationToken = default);
    Task<Result<PatientDto>> UpdateAsync(Guid id, UpdatePatientDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
