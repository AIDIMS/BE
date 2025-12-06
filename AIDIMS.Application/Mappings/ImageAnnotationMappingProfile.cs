using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class ImageAnnotationMappingProfile : Profile
{
    public ImageAnnotationMappingProfile()
    {
        // Entity to DTO
        CreateMap<ImageAnnotation, ImageAnnotationDto>()
            .ForMember(dest => dest.InstanceSopInstanceUid,
                opt => opt.MapFrom(src => src.Instance != null ? src.Instance.SopInstanceUid : null));

        // DTO to Entity
        CreateMap<CreateImageAnnotationDto, ImageAnnotation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Instance, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        CreateMap<UpdateImageAnnotationDto, ImageAnnotation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.InstanceId, opt => opt.Ignore())
            .ForMember(dest => dest.Instance, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
    }
}

