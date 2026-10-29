USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetDashboard AS SELECT (SELECT COUNT(*) FROM dbo.Patients WHERE IsActive=1) Patients,(SELECT COUNT(*) FROM dbo.Doctors WHERE IsActive=1) Doctors,(SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentDate=CONVERT(date,GETDATE())) TodayAppointments,(SELECT SUM(GrandTotal) FROM dbo.Bills) BilledRevenue;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetPatients AS SELECT * FROM dbo.Patients ORDER BY CreatedAt DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetDoctors @Search NVARCHAR(200)=NULL,@DepartmentId INT=NULL,@Status NVARCHAR(20)=NULL AS BEGIN SET NOCOUNT ON; SELECT d.DoctorId, d.UserId, d.DoctorCode, d.FirstName, d.LastName, d.Gender, d.DateOfBirth, d.Phone, d.Email, d.Qualification, d.SpecializationId, d.ExperienceYears, d.ConsultationFee, d.AboutDoctor, d.ProfileImage, d.IsActive, d.CreatedAt, d.UpdatedAt, s.SpecializationName, dep.DepartmentName, dep.DepartmentId FROM dbo.Doctors d JOIN dbo.Specializations s ON s.SpecializationId=d.SpecializationId JOIN dbo.Departments dep ON dep.DepartmentId=s.DepartmentId WHERE (@Search IS NULL OR LTRIM(RTRIM(@Search)) = N'' OR d.FirstName LIKE N'%'+LTRIM(RTRIM(@Search))+N'%' OR d.LastName LIKE N'%'+LTRIM(RTRIM(@Search))+N'%' OR d.DoctorCode LIKE N'%'+LTRIM(RTRIM(@Search))+N'%' OR d.Email LIKE N'%'+LTRIM(RTRIM(@Search))+N'%') AND (@DepartmentId IS NULL OR dep.DepartmentId=@DepartmentId) AND (@Status IS NULL OR @Status = N'All' OR (@Status = N'Active' AND d.IsActive = 1) OR (@Status = N'Inactive' AND d.IsActive = 0)) ORDER BY d.DoctorId DESC; END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetReceptionists AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ReceptionistId,r.UserId,r.EmployeeCode,r.FirstName,r.LastName,r.Gender,r.Phone,r.Email,r.JoiningDate,r.IsActive,
           u.Username,u.IsActive AS UserIsActive
    FROM dbo.Receptionists r LEFT JOIN dbo.Users u ON u.UserId=r.UserId
    ORDER BY r.FirstName,r.LastName;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetReceptionistById @ReceptionistId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ReceptionistId,r.UserId,r.EmployeeCode,r.FirstName,r.LastName,r.Gender,r.Phone,r.Email,r.JoiningDate,r.IsActive,
           u.Username,u.IsActive AS UserIsActive
    FROM dbo.Receptionists r LEFT JOIN dbo.Users u ON u.UserId=r.UserId
    WHERE r.ReceptionistId=@ReceptionistId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_UpdateReceptionist
    @ReceptionistId INT,@Username NVARCHAR(100)=NULL,@Email NVARCHAR(150),@EmployeeCode NVARCHAR(30),
    @FirstName NVARCHAR(100),@LastName NVARCHAR(100)=NULL,@Gender NVARCHAR(20),@Phone NVARCHAR(20),@JoiningDate DATE=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    UPDATE dbo.Receptionists SET EmployeeCode=@EmployeeCode,FirstName=@FirstName,LastName=@LastName,Gender=@Gender,Phone=@Phone,Email=@Email,JoiningDate=@JoiningDate WHERE ReceptionistId=@ReceptionistId;
    IF @@ROWCOUNT=0 THROW 50005,N'Receptionist not found.',1;
    UPDATE u SET u.Username=COALESCE(NULLIF(@Username,N''),u.Username),u.Email=@Email,u.UpdatedAt=SYSDATETIME()
    FROM dbo.Users u JOIN dbo.Receptionists r ON r.UserId=u.UserId WHERE r.ReceptionistId=@ReceptionistId;
    COMMIT;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_DeactivateReceptionist @ReceptionistId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    UPDATE r SET IsActive=0 FROM dbo.Receptionists r WHERE r.ReceptionistId=@ReceptionistId;
    IF @@ROWCOUNT=0 THROW 50005,N'Receptionist not found.',1;
    UPDATE u SET IsActive=0,UpdatedAt=SYSDATETIME() FROM dbo.Users u JOIN dbo.Receptionists r ON r.UserId=u.UserId WHERE r.ReceptionistId=@ReceptionistId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_ActivateReceptionist @ReceptionistId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    UPDATE r SET IsActive=1 FROM dbo.Receptionists r WHERE r.ReceptionistId=@ReceptionistId;
    IF @@ROWCOUNT=0 THROW 50005,N'Receptionist not found.',1;
    UPDATE u SET IsActive=1,UpdatedAt=SYSDATETIME() FROM dbo.Users u JOIN dbo.Receptionists r ON r.UserId=u.UserId WHERE r.ReceptionistId=@ReceptionistId;
END;
GO
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_CreateReceptionist
    @Username NVARCHAR(100), @Email NVARCHAR(150), @PasswordHash NVARCHAR(500),
    @EmployeeCode NVARCHAR(30), @FirstName NVARCHAR(100), @LastName NVARCHAR(100)=NULL,
    @Gender NVARCHAR(20), @Phone NVARCHAR(20), @JoiningDate DATE=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @RoleId INT=(SELECT RoleId FROM dbo.Roles WHERE RoleName=N'Receptionist' AND IsActive=1);
    IF @RoleId IS NULL THROW 54003,N'Receptionist role is not configured.',1;
    INSERT dbo.Users(RoleId,Username,Email,PasswordHash,IsActive) VALUES(@RoleId,@Username,@Email,@PasswordHash,1);
    DECLARE @UserId INT=CONVERT(INT,SCOPE_IDENTITY());
    INSERT dbo.Receptionists(UserId,EmployeeCode,FirstName,LastName,Gender,Phone,Email,JoiningDate)
    VALUES(@UserId,@EmployeeCode,@FirstName,@LastName,@Gender,@Phone,@Email,@JoiningDate);
    SELECT @UserId UserId,CONVERT(INT,SCOPE_IDENTITY()) ReceptionistId;
    COMMIT;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetDepartments AS SELECT * FROM dbo.Departments ORDER BY DepartmentName;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetSpecializations AS SELECT s.*,d.DepartmentName FROM dbo.Specializations s JOIN dbo.Departments d ON d.DepartmentId=s.DepartmentId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetSchedules AS SELECT ds.*,d.DoctorCode,d.FirstName,d.LastName FROM dbo.DoctorSchedules ds JOIN dbo.Doctors d ON d.DoctorId=ds.DoctorId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetAppointments AS SELECT * FROM dbo.Appointments ORDER BY AppointmentDate DESC,StartTime DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetMedicalRecords AS SELECT * FROM dbo.MedicalRecords ORDER BY RecordDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetPrescriptions AS SELECT * FROM dbo.Prescriptions ORDER BY PrescriptionDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetMedicalTests AS SELECT * FROM dbo.MedicalTests ORDER BY TestDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetBills AS SELECT * FROM dbo.Bills ORDER BY BillDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetPayments AS SELECT * FROM dbo.Payments ORDER BY PaymentDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetNotifications AS SELECT * FROM dbo.Notifications ORDER BY CreatedAt DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Admin_GetAuditLogs AS SELECT * FROM dbo.AuditLogs ORDER BY CreatedAt DESC;
GO
