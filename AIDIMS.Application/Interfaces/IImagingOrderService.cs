using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IImagingOrderService
{
    Task<Result<ImagingOrderDto>> CreateAsync(CreateImagingOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result<ImagingOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ImagingOrderDto>>> GetAllAsync(PaginationParams paginationParams, SearchImagingOrderDto filters, CancellationToken cancellationToken = default);
    Task<Result<ImagingOrderDto>> UpdateAsync(Guid id, UpdateImagingOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
