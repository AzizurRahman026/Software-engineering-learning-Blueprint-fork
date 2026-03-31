using Application.Models.Notifications;

namespace Application.Common.Interfaces.Services;

public interface INotificationService
{
    Task SendToAllAsync(NotificationDto notification);
}
