using Application.Common.Interfaces.Services;
using Application.Models.Notifications;
using Infrastructure.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.SignalR.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToAllAsync(NotificationDto notification)
    {
        _logger.LogInformation(
            "Broadcasting notification to all clients. Type={Type} Message={Message}",
            notification.Type,
            notification.Message);

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
