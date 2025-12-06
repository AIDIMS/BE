using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IImageAnnotationService
{
    Task<Result<ImageAnnotationDto>> CreateAsync(CreateImageAnnotationDto dto, CancellationToken cancellationToken = default);
    Task<Result<ImageAnnotationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ImageAnnotationDto>>> GetAllAsync(PaginationParams paginationParams, SearchImageAnnotationDto filters, CancellationToken cancellationToken = default);
    Task<Result<ImageAnnotationDto>> UpdateAsync(Guid id, UpdateImageAnnotationDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ImageAnnotationDto>>> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default);
}

