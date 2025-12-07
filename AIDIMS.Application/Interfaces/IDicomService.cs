using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IDicomService
{
    Task<DicomUploadResultDto?> UploadInstanceAsync(DicomUploadDto dicom, CancellationToken cancellationToken = default);
    Task<IEnumerable<DicomInstanceDto>> GetInstancesByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<byte[]?> DownloadInstanceAsync(string instanceId, CancellationToken cancellationToken = default);
    Task<byte[]?> GetInstancePreviewAsync(string instanceId, CancellationToken cancellationToken = default);
}
