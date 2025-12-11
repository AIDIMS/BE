using System.Text.Json.Serialization;
using AIDIMS.Application.Common;

namespace AIDIMS.Application.DTOs;

public class DiagnosisDto
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public string FinalDiagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }

    [JsonConverter(typeof(DiagnosisReportStatusStringConverter))]
    public string ReportStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Study information
    public string? StudyDescription { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
}

public class CreateDiagnosisDto
{
    public Guid StudyId { get; set; }
    public string FinalDiagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }

    [JsonConverter(typeof(DiagnosisReportStatusStringConverter))]
    public string ReportStatus { get; set; } = "Draft";
}

public class UpdateDiagnosisDto
{
    public string FinalDiagnosis { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }

    [JsonConverter(typeof(DiagnosisReportStatusStringConverter))]
    public string ReportStatus { get; set; } = string.Empty;
}

public class SearchDiagnosisDto
{
    public Guid? StudyId { get; set; }
    public string? ReportStatus { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? DoctorId { get; set; }
}

