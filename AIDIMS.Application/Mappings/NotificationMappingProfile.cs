using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Entities;
using AutoMapper;

namespace AIDIMS.Application.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
        CreateMap<CreateNotificationDto, Notification>();
    }
}
