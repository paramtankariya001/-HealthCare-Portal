using HealthCare_Portal.Data;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;

namespace HealthCare_Portal.Repositories;

public sealed class NotificationRepository(DbHelper db) : INotificationRepository
{
    public async Task<long> CreateAsync(int userId, string title, string message, string type, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@UserId", userId),
            DbHelper.Parameter("@Title", title),
            DbHelper.Parameter("@Message", message),
            DbHelper.Parameter("@NotificationType", type)
        };

        var result = await db.ExecuteScalarAsync("dbo.usp_Notification_Create", parameters, ct);
        return Convert.ToInt64(result);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(int userId, bool unreadOnly = false, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@UserId", userId),
            DbHelper.Parameter("@UnreadOnly", unreadOnly)
        };

        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Notification_GetByUser", parameters, ct);
        return rows.Select(MapNotification).ToList();
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, int userId, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@NotificationId", notificationId),
            DbHelper.Parameter("@UserId", userId)
        };

        var affected = await db.ExecuteNonQueryAsync("dbo.usp_Notification_MarkAsRead", parameters, ct);
        return affected > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@UserId", userId)
        };

        var affected = await db.ExecuteNonQueryAsync("dbo.usp_Notification_MarkAllAsRead", parameters, ct);
        return affected > 0;
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@UserId", userId)
        };

        var result = await db.ExecuteScalarAsync("dbo.usp_Notification_GetUnreadCount", parameters, ct);
        return Convert.ToInt32(result);
    }

    private static Notification MapNotification(Dictionary<string, object?> r)
    {
        return new Notification
        {
            NotificationId = Convert.ToInt64(r["NotificationId"]),
            UserId = Convert.ToInt32(r["UserId"]),
            Title = Convert.ToString(r["Title"]) ?? string.Empty,
            Message = Convert.ToString(r["Message"]) ?? string.Empty,
            NotificationType = Convert.ToString(r["NotificationType"]) ?? "System",
            IsRead = Convert.ToBoolean(r["IsRead"]),
            CreatedAt = Convert.ToDateTime(r["CreatedAt"])
        };
    }
}
