using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace AIDIMS.Application.UseCases;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<Hub>? _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHubContext<Hub>? hubContext = null)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<Result<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = _mapper.Map<Notification>(dto);
        notification.IsSent = false;

        var createdNotification = await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var notificationDto = _mapper.Map<NotificationDto>(createdNotification);

        // Send realtime notification via SignalR
        await SendNotificationAsync(notificationDto, cancellationToken);

        return Result<NotificationDto>.Success(notificationDto);
    }

    public async Task<Result<IEnumerable<NotificationDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);
        var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        return Result<IEnumerable<NotificationDto>>.Success(notificationDtos);
    }

    public async Task<Result<IEnumerable<NotificationDto>>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, cancellationToken);
        var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        return Result<IEnumerable<NotificationDto>>.Success(notificationDtos);
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
        return Result<int>.Success(count);
    }

    public async Task<Result> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        return Result.Success();
    }

    public async Task SendNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        if (_hubContext != null)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"user_{notification.UserId}")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                // Mark as sent
                var entity = await _notificationRepository.GetByIdAsync(notification.Id, cancellationToken);
                if (entity != null)
                {
                    entity.IsSent = true;
                    await _notificationRepository.UpdateAsync(entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch
            {
                // Log error but don't throw - notification is saved in DB
            }
        }
    }
}
