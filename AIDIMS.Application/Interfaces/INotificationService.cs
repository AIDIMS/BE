using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface INotificationService
{
    Task<Result<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NotificationDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NotificationDto>>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
}
