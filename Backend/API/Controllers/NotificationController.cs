using Application.Common.Interfaces.Services;
using Application.Models.Notifications;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // ── POST /api/notification/test ──────────────────────────────────────────
    // Sends a test notification to ALL connected SignalR clients.
    // Body: { "message": "Hello from server!", "type": 0 }
    // type: 0=Info, 1=Warning, 2=Success, 3=Error
    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification([FromBody] NotificationDto notification)
    {
        await _notificationService.SendToAllAsync(notification);
        return Ok(new { status = "Notification sent to all clients", notification });
    }

    // ── GET /api/notification/ping ──────────────────────────────────────────
    // Quick test — no body needed, just hit this endpoint.
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var notification = new NotificationDto
        {
            Message = "🔔 Ping! Server is alive.",
            Type = NotificationType.Info
        };

        await _notificationService.SendToAllAsync(notification);
        return Ok(new { status = "Ping notification sent", notification });
    }
}
