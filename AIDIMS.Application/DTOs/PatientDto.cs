using System.Text.Json.Serialization;
using AIDIMS.Application.Common;

namespace AIDIMS.Application.DTOs;

public class PatientDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class CreatePatientDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    [JsonConverter(typeof(GenderStringConverter))]
    public string Gender { get; set; } = default;
    
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class UpdatePatientDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    [JsonConverter(typeof(GenderStringConverter))]
    public string Gender { get; set; } = default;
    
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class SearchPatientDto
{
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
}


