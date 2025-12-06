using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;
using System.Text.Json;

namespace AIDIMS.Application.Mappings;

public class ImageAnnotationMappingProfile : Profile
{
    public ImageAnnotationMappingProfile()
    {
        // Entity to DTO
        CreateMap<ImageAnnotation, ImageAnnotationDto>()
            .ForMember(dest => dest.InstanceSopInstanceUid,
                opt => opt.MapFrom(src => src.Instance != null ? src.Instance.SopInstanceUid : null))
            .ForMember(dest => dest.ParsedAnnotationData,
                opt => opt.MapFrom(src => ParseAnnotationData(src.AnnotationType, src.AnnotationData)));

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

    private static object? ParseAnnotationData(string annotationType, string annotationData)
    {
        if (string.IsNullOrWhiteSpace(annotationData))
            return null;

        try
        {
            if (annotationType == "BoundingBoxAnnotation")
            {
                var parsedData = JsonSerializer.Deserialize<List<BoundingBoxAnnotationData>>(annotationData);
                return parsedData;
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

