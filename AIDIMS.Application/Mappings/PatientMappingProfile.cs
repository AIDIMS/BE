using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        // Entity to DTO
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.LastVisitDate,
                opt => opt.MapFrom(src => src.Visits
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => (DateTime?)v.CreatedAt)
                    .FirstOrDefault()));

        CreateMap<Patient, PatientDetailsDto>()
            .ForMember(dest => dest.LastVisitDate,
                opt => opt.MapFrom(src => src.Visits
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => (DateTime?)v.CreatedAt)
                    .FirstOrDefault()));

        CreateMap<PatientVisit, PatientVisitDto>()
            .ForMember(dest => dest.AssignedDoctorName, opt => opt.MapFrom(src => src.AssignedDoctor.FullName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ImagingOrder, ImagingOrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<PatientVisit, PatientVisitDto>()
            .ForMember(dest => dest.AssignedDoctorName,
                    opt => opt.MapFrom(src => src.AssignedDoctor.FullName))
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Patient.FullName));

        // DTO to Entity
        CreateMap<CreatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PatientCode, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Unspecified)))
            .ForMember(dest => dest.Visits, opt => opt.Ignore())
            .ForMember(dest => dest.Studies, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        CreateMap<UpdatePatientDto, Patient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PatientCode, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Unspecified)))
            .ForMember(dest => dest.Visits, opt => opt.Ignore())
            .ForMember(dest => dest.Studies, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
    }
}
