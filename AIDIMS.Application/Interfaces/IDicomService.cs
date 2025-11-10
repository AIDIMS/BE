using AIDIMS.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace AIDIMS.Application.Interfaces;

public interface IDicomService
{
    Task<DicomUploadResultDto?> UploadInstanceAsync(DicomUploadDto dicom);
}
