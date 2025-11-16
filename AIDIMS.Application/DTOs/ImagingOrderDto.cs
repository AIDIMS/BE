using System.Text.Json.Serialization;
using AIDIMS.Application.Common;

namespace AIDIMS.Application.DTOs;

public class ImagingOrderDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid RequestingDoctorId { get; set; }
    public string RequestingDoctorName { get; set; } = string.Empty;
    public string ModalityRequested { get; set; } = string.Empty;
    public string BodyPartRequested { get; set; } = string.Empty;
    public string? ReasonForStudy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateImagingOrderDto
{
    public Guid VisitId { get; set; }
    public Guid RequestingDoctorId { get; set; }
    
    [JsonConverter(typeof(ModalityStringConverter))]
    public string ModalityRequested { get; set; } = string.Empty;
    
    [JsonConverter(typeof(BodyPartStringConverter))]
    public string BodyPartRequested { get; set; } = string.Empty;
    
    public string? ReasonForStudy { get; set; }
}

public class UpdateImagingOrderDto
{
    [JsonConverter(typeof(ModalityStringConverter))]
    public string? ModalityRequested { get; set; }
    
    [JsonConverter(typeof(BodyPartStringConverter))]
    public string? BodyPartRequested { get; set; }
    
    public string? ReasonForStudy { get; set; }
    
    [JsonConverter(typeof(ImagingOrderStatusStringConverter))]
    public string? Status { get; set; }
}

public class SearchImagingOrderDto
{
    public Guid? VisitId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? RequestingDoctorId { get; set; }
    public string? Modality { get; set; }
    public string? BodyPart { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
