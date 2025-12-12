namespace AIDIMS.Domain.Enums;

public enum NotificationType
{
    VisitCreated,           // Khi tạo ca khám mới
    ImagingOrderAssigned,   // Khi tạo chỉ định chụp chiếu (cho KTV)
    AiAnalysisCompleted,    // Khi AI phân tích xong (cho bác sĩ)
    DiagnosisCompleted,     // Khi bác sĩ hoàn thành chẩn đoán
    General                 // Thông báo chung
}
