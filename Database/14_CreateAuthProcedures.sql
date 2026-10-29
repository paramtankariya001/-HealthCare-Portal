USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_User_Login @Email NVARCHAR(150), @PasswordHash NVARCHAR(500) AS
BEGIN
 SET NOCOUNT ON;
 IF NULLIF(LTRIM(RTRIM(@Email)),N'') IS NULL OR NULLIF(@PasswordHash,N'') IS NULL THROW 50001,N'Email and password hash are required.',1;
 SELECT TOP (1) u.UserId,u.RoleId,r.RoleName,u.Username,u.Email,u.PasswordHash,u.IsActive,
        p.PatientId,d.DoctorId,rec.ReceptionistId
 FROM dbo.Users u
 JOIN dbo.Roles r ON r.RoleId=u.RoleId
 LEFT JOIN dbo.Patients p ON p.UserId=u.UserId
 LEFT JOIN dbo.Doctors d ON d.UserId=u.UserId
 LEFT JOIN dbo.Receptionists rec ON rec.UserId=u.UserId
 WHERE (u.Email=@Email OR u.Username=@Email)
   AND u.PasswordHash=@PasswordHash
   AND u.IsActive=1
   AND r.IsActive=1
 ORDER BY CASE WHEN u.Username=@Email THEN 0 ELSE 1 END;
END;
GO
