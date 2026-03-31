using Domain.Enums;

namespace Application.Models.Notifications;

public class NotificationDto
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
