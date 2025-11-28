using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class PatientVisitMappingProfile : Profile
{
    public PatientVisitMappingProfile()
    {
        CreateMap<PatientVisit, PatientVisitDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PatientCode, opt => opt.MapFrom(src => src.Patient != null ? src.Patient.PatientCode : string.Empty))
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty))
            .ForMember(dest => dest.AssignedDoctorName, opt => opt.MapFrom(src => src.AssignedDoctor != null ? src.AssignedDoctor.FullName : string.Empty));

        CreateMap<CreatePatientVisitDto, PatientVisit>();
    }
}
