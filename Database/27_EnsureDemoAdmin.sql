USE HealthCarePortalDB;
GO
-- DEVELOPMENT / DEMONSTRATION ONLY.
-- Default Admin credentials: Login ID `admin`, password `admin`.
-- Change this password before any production deployment.
-- This script does not recreate the database or other user accounts.
DECLARE @Hash NVARCHAR(500) = N'PBKDF2-SHA256:100000:mfKEkWLCBMfbNutns17IHA==:lVTUN/xWUnbTSsNg/VXq7/HsaaYIwFfPUJGdm+Unwic=';
DECLARE @RoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Admin' AND IsActive = 1);
IF @RoleId IS NULL THROW 54002, N'Admin role is not configured.', 1;
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
    UPDATE dbo.Users
    SET RoleId = @RoleId, PasswordHash = @Hash, IsActive = 1, UpdatedAt = SYSDATETIME()
    WHERE Username = N'admin';
ELSE
    INSERT dbo.Users (RoleId, Username, Email, PasswordHash, IsActive)
    VALUES (@RoleId, N'admin', N'admin@healthcare.local', @Hash, 1);
GO
