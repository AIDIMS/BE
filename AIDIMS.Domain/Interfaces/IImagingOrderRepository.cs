using AIDIMS.Domain.Common;
using AIDIMS.Domain.Entities;

namespace AIDIMS.Domain.Interfaces;

public interface IImagingOrderRepository : IRepository<ImagingOrder>
{
    Task<IEnumerable<ImagingOrder>> GetAllWithStudiesAsync(CancellationToken cancellationToken = default);
}
