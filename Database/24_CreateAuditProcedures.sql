USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AuditLog_Create @UserId INT=NULL,@RoleName NVARCHAR(50)=NULL,@Action NVARCHAR(100),@ModuleName NVARCHAR(100),@Description NVARCHAR(1000)=NULL,@RecordId NVARCHAR(100)=NULL,@IPAddress NVARCHAR(50)=NULL AS INSERT dbo.AuditLogs(UserId,RoleName,Action,ModuleName,Description,RecordId,IPAddress) VALUES(@UserId,@RoleName,@Action,@ModuleName,@Description,@RecordId,@IPAddress); SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AuditLogId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AuditLog_GetAll @FromDate DATE=NULL,@ToDate DATE=NULL AS SELECT * FROM dbo.AuditLogs WHERE (@FromDate IS NULL OR CONVERT(date,CreatedAt)>=@FromDate) AND (@ToDate IS NULL OR CONVERT(date,CreatedAt)<=@ToDate) ORDER BY CreatedAt DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AuditLog_GetByUser @UserId INT AS SELECT * FROM dbo.AuditLogs WHERE UserId=@UserId ORDER BY CreatedAt DESC;
GO
