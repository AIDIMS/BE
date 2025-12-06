using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class DiagnosisMappingProfile : Profile
{
    public DiagnosisMappingProfile()
    {
        // Entity to DTO
        CreateMap<Diagnosis, DiagnosisDto>()
            .ForMember(dest => dest.StudyDescription,
                opt => opt.MapFrom(src => src.Study != null ? src.Study.StudyDescription : null))
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Study != null && src.Study.Patient != null 
                    ? src.Study.Patient.FullName : null))
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Study != null && src.Study.AssignedDoctor != null 
                    ? $"{src.Study.AssignedDoctor.FirstName} {src.Study.AssignedDoctor.LastName}".Trim() : null))
            .ForMember(dest => dest.ReportStatus,
                opt => opt.MapFrom(src => src.ReportStatus.ToString()));

        // DTO to Entity
        CreateMap<CreateDiagnosisDto, Diagnosis>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Study, opt => opt.Ignore())
            .ForMember(dest => dest.ReportStatus,
                opt => opt.MapFrom(src => Enum.Parse<DiagnosisReportStatus>(src.ReportStatus, true)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        CreateMap<UpdateDiagnosisDto, Diagnosis>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StudyId, opt => opt.Ignore())
            .ForMember(dest => dest.Study, opt => opt.Ignore())
            .ForMember(dest => dest.ReportStatus,
                opt => opt.MapFrom(src => Enum.Parse<DiagnosisReportStatus>(src.ReportStatus, true)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
    }
}

