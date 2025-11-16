using System.Text.Json.Serialization;
using AIDIMS.Application.Common;

namespace AIDIMS.Application.DTOs;

public class PatientVisitDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid AssignedDoctorId { get; set; }
    public string AssignedDoctorName { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePatientVisitDto
{
    public Guid PatientId { get; set; }
    public Guid AssignedDoctorId { get; set; }
    public string? Symptoms { get; set; }
}

public class UpdatePatientVisitDto
{
    public Guid? AssignedDoctorId { get; set; }
    public string? Symptoms { get; set; }
    
    [JsonConverter(typeof(PatientVisitStatusStringConverter))]
    public string? Status { get; set; }
}

public class SearchPatientVisitDto
{
    public Guid? PatientId { get; set; }
    public Guid? AssignedDoctorId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
