using AIDIMS.Domain.Common;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Domain.Entities;

public class Diagnosis : BaseAuditableEntity
{
    public Guid StudyId { get; set; }
    public DicomStudy Study { get; set; } = default!;

    public string FinalDiagnosis { get; set; } = string.Empty;
    public DiagnosisReportStatus ReportStatus { get; set; }
}
