using HealthCare_Portal.Models;

namespace HealthCare_Portal.Services.Interfaces;

public interface INotificationService
{
    Task<OperationResult<long>> SendNotificationAsync(int userId, string title, string message, string type, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Notification>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task<OperationResult> MarkAsReadAsync(long notificationId, int userId, CancellationToken cancellationToken = default);
    Task<OperationResult> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}
