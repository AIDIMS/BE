using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class ImagingOrderMappingProfile : Profile
{
    public ImagingOrderMappingProfile()
    {
        CreateMap<ImagingOrder, ImagingOrderDto>()
            .ForMember(dest => dest.ModalityRequested, opt => opt.MapFrom(src => src.ModalityRequested.ToString()))
            .ForMember(dest => dest.BodyPartRequested, opt => opt.MapFrom(src => src.BodyPartRequested.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PatientId, opt => opt.Ignore())
            .ForMember(dest => dest.PatientName, opt => opt.Ignore())
            .ForMember(dest => dest.RequestingDoctorName, opt => opt.Ignore());
    }
}
