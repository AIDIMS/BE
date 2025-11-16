using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace AIDIMS.Application.DTOs;

public class DicomUploadDto
{
    public required IFormFile File { get; set; }
    public Guid? OrderId { get; set; } // Optional: link to existing order
    public Guid? PatientId { get; set; } // Optional: link to existing patient
}

public class DicomUploadResultDto
{
    [JsonPropertyName("ID")]
    public string ID { get; set; } = string.Empty;
    
    [JsonPropertyName("ParentPatient")]
    public string ParentPatient { get; set; } = string.Empty;
    
    [JsonPropertyName("ParentSeries")]
    public string ParentSeries { get; set; } = string.Empty;
    
    [JsonPropertyName("ParentStudy")]
    public string ParentStudy { get; set; } = string.Empty;
    
    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;
    
    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;
}

// DTOs for Orthanc metadata
public class OrthancPatientDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("MainDicomTags")]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}

public class OrthancStudyDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("MainDicomTags")]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
    
    [JsonPropertyName("PatientMainDicomTags")]
    public Dictionary<string, string> PatientMainDicomTags { get; set; } = new();
}

public class OrthancSeriesDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("MainDicomTags")]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}

public class OrthancInstanceDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("MainDicomTags")]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}
