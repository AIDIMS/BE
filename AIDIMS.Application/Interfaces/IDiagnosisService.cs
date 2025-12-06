using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IDiagnosisService
{
    Task<Result<DiagnosisDto>> CreateAsync(CreateDiagnosisDto dto, CancellationToken cancellationToken = default);
    Task<Result<DiagnosisDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DiagnosisDto>> GetByStudyIdAsync(Guid studyId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<DiagnosisDto>>> GetAllAsync(PaginationParams paginationParams, SearchDiagnosisDto filters, CancellationToken cancellationToken = default);
    Task<Result<DiagnosisDto>> UpdateAsync(Guid id, UpdateDiagnosisDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

