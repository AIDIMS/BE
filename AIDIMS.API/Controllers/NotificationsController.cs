using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIDIMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("my-notifications")]
    public async Task<ActionResult<Result<IEnumerable<NotificationDto>>>> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(Result<IEnumerable<NotificationDto>>.Failure("User not authenticated"));
        }

        var result = await _notificationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<Result<IEnumerable<NotificationDto>>>> GetUnread(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(Result<IEnumerable<NotificationDto>>.Failure("User not authenticated"));
        }

        var result = await _notificationService.GetUnreadByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<Result<int>>> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(Result<int>.Failure("User not authenticated"));
        }

        var result = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/mark-read")]
    public async Task<ActionResult<Result>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("mark-all-read")]
    public async Task<ActionResult<Result>> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(Result.Failure("User not authenticated"));
        }

        var result = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<NotificationDto>>> Create(
        CreateNotificationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }
}
