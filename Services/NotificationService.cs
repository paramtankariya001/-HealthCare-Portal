using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;

namespace HealthCare_Portal.Services;

public sealed class NotificationService(
    INotificationRepository repository,
    ILogger<NotificationService> logger) : INotificationService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Appointment", "Prescription", "Medical Test", "Payment", "System", "Reminder"
    };

    public async Task<OperationResult<long>> SendNotificationAsync(int userId, string title, string message, string type, CancellationToken ct = default)
    {
        if (userId <= 0)
            return OperationResult<long>.Fail("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<long>.Fail("Title is required.");

        if (string.IsNullOrWhiteSpace(message))
            return OperationResult<long>.Fail("Message is required.");

        if (!AllowedTypes.Contains(type))
            type = "System";

        try
        {
            var id = await repository.CreateAsync(userId, title.Trim(), message.Trim(), type, ct);
            return OperationResult<long>.Ok(id, "Notification sent.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification to user {UserId}.", userId);
            return OperationResult<long>.Fail("Could not send notification.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Notification>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, CancellationToken ct = default)
    {
        if (userId <= 0)
            return OperationResult<IReadOnlyList<Notification>>.Fail("Invalid user session.");

        try
        {
            var list = await repository.GetByUserAsync(userId, unreadOnly, ct);
            return OperationResult<IReadOnlyList<Notification>>.Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get notifications for user {UserId}.", userId);
            return OperationResult<IReadOnlyList<Notification>>.Fail("Failed to retrieve notifications.");
        }
    }

    public async Task<OperationResult> MarkAsReadAsync(long notificationId, int userId, CancellationToken ct = default)
    {
        if (notificationId <= 0 || userId <= 0)
            return OperationResult.Fail("Invalid request.");

        try
        {
            var success = await repository.MarkAsReadAsync(notificationId, userId, ct);
            return success ? OperationResult.Ok("Notification marked as read.") : OperationResult.Fail("Notification not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark notification {NotificationId} as read.", notificationId);
            return OperationResult.Fail("Error updating notification status.");
        }
    }

    public async Task<OperationResult> MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0)
            return OperationResult.Fail("Invalid user session.");

        try
        {
            await repository.MarkAllAsReadAsync(userId, ct);
            return OperationResult.Ok("All notifications marked as read.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}.", userId);
            return OperationResult.Fail("Error updating notifications.");
        }
    }

    public async Task<OperationResult<int>> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0)
            return OperationResult<int>.Ok(0);

        try
        {
            var count = await repository.GetUnreadCountAsync(userId, ct);
            return OperationResult<int>.Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get unread notification count for user {UserId}.", userId);
            return OperationResult<int>.Ok(0);
        }
    }
}
