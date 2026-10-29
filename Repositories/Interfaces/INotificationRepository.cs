using HealthCare_Portal.Models;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<long> CreateAsync(int userId, string title, string message, string type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserAsync(int userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(long notificationId, int userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}
