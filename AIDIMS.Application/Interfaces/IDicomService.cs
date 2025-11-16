using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IDicomService
{
    Task<DicomUploadResultDto?> UploadInstanceAsync(DicomUploadDto dicom, CancellationToken cancellationToken = default);
}
