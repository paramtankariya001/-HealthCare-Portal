namespace HealthCare_Portal.Models;
public sealed class Notification { public long NotificationId { get; set; } public int UserId { get; set; } public string Title { get; set; } = ""; public string Message { get; set; } = ""; public string NotificationType { get; set; } = ""; public bool IsRead { get; set; } public DateTime CreatedAt { get; set; } }
