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
            .ForMember(dest => dest.PatientName, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedDoctorName, opt => opt.Ignore());

        CreateMap<CreatePatientVisitDto, PatientVisit>();
    }
}
