using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IPatientVisitService
{
    Task<Result<PatientVisitDto>> CreateAsync(CreatePatientVisitDto dto, CancellationToken cancellationToken = default);
    Task<Result<PatientVisitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PatientVisitDto>>> GetAllAsync(PaginationParams paginationParams, SearchPatientVisitDto filters, CancellationToken cancellationToken = default);
    Task<Result<PatientVisitDto>> UpdateAsync(Guid id, UpdatePatientVisitDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
