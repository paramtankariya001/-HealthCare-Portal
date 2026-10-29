USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Notification_Create 
    @UserId INT,
    @Title NVARCHAR(200),
    @Message NVARCHAR(1000),
    @NotificationType NVARCHAR(50) 
AS 
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Notifications (UserId, Title, Message, NotificationType) 
    VALUES (@UserId, @Title, @Message, @NotificationType); 
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS NotificationId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_GetByUser 
    @UserId INT,
    @UnreadOnly BIT = 0 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT * 
    FROM dbo.Notifications 
    WHERE UserId = @UserId 
      AND (@UnreadOnly = 0 OR IsRead = 0) 
    ORDER BY CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_MarkAsRead 
    @NotificationId BIGINT,
    @UserId INT 
AS 
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Notifications 
    SET IsRead = 1 
    WHERE NotificationId = @NotificationId 
      AND UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_MarkAllAsRead 
    @UserId INT 
AS 
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Notifications 
    SET IsRead = 1 
    WHERE UserId = @UserId 
      AND IsRead = 0;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_GetUnreadCount
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) 
    FROM dbo.Notifications 
    WHERE UserId = @UserId 
      AND IsRead = 0;
END;
GO
