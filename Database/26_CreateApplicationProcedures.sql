USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_User_GetByEmail @Email NVARCHAR(150) AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) u.UserId,u.RoleId,u.Username,u.Email,u.PasswordHash,u.IsActive,u.LastLoginAt,u.CreatedAt,u.UpdatedAt,
           r.RoleName,p.PatientId,d.DoctorId,rec.ReceptionistId
    FROM dbo.Users u
    JOIN dbo.Roles r ON r.RoleId=u.RoleId
    LEFT JOIN dbo.Patients p ON p.UserId=u.UserId
    LEFT JOIN dbo.Doctors d ON d.UserId=u.UserId
    LEFT JOIN dbo.Receptionists rec ON rec.UserId=u.UserId
    WHERE u.Username=@Email OR u.Email=@Email
    ORDER BY CASE WHEN u.Username=@Email THEN 0 ELSE 1 END;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_User_RegisterPatient
    @Username NVARCHAR(100), @Email NVARCHAR(150), @PasswordHash NVARCHAR(500),
    @PatientCode NVARCHAR(30), @FirstName NVARCHAR(100), @LastName NVARCHAR(100)=NULL,
    @Gender NVARCHAR(20), @DateOfBirth DATE, @BloodGroup NVARCHAR(10)=NULL,
    @Phone NVARCHAR(20), @Address NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @RoleId INT=(SELECT RoleId FROM dbo.Roles WHERE RoleName=N'Patient' AND IsActive=1);
    IF @RoleId IS NULL THROW 54001,N'Patient role is not configured.',1;
    INSERT dbo.Users(RoleId,Username,Email,PasswordHash) VALUES(@RoleId,@Username,@Email,@PasswordHash);
    DECLARE @UserId INT=CONVERT(INT,SCOPE_IDENTITY());
    INSERT dbo.Patients(UserId,PatientCode,FirstName,LastName,Gender,DateOfBirth,BloodGroup,Phone,Email,Address)
    VALUES(@UserId,@PatientCode,@FirstName,@LastName,@Gender,@DateOfBirth,@BloodGroup,@Phone,@Email,@Address);
    SELECT @UserId UserId,CONVERT(INT,SCOPE_IDENTITY()) PatientId;
    COMMIT;
END;
GO
