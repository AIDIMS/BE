using Microsoft.AspNetCore.Http;

namespace AIDIMS.Application.DTOs;

public class DicomUploadDto
{
    public IFormFile File { get; set; }
}

public class DicomUploadResultDto
{
    public string ID { get; set; }
    public string ParentPatient { get; set; }
    public string ParentSeries { get; set; }
    public string ParentStudy { get; set; }
    public string Path { get; set; }
    public string Status { get; set; }
}
