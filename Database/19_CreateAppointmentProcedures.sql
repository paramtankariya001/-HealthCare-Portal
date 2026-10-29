USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_CheckSlotAvailability @DoctorId INT,@AppointmentDate DATE,@StartTime TIME,@EndTime TIME AS
BEGIN SET NOCOUNT ON; SET DATEFIRST 1; DECLARE @Day TINYINT=DATEPART(WEEKDAY,@AppointmentDate); SELECT CAST(CASE WHEN NOT EXISTS(SELECT 1 FROM dbo.Doctors WHERE DoctorId=@DoctorId AND IsActive=1) OR NOT EXISTS(SELECT 1 FROM dbo.DoctorSchedules s WHERE s.DoctorId=@DoctorId AND s.DayOfWeek=@Day AND s.IsActive=1 AND @StartTime>=s.StartTime AND @EndTime<=s.EndTime AND DATEDIFF(MINUTE,@StartTime,@EndTime)=s.SlotDurationMinutes AND DATEDIFF(MINUTE,s.StartTime,@StartTime)%s.SlotDurationMinutes=0 AND (@StartTime>=ISNULL(s.BreakEndTime,'00:00') OR @EndTime<=ISNULL(s.BreakStartTime,'00:00'))) OR EXISTS(SELECT 1 FROM dbo.Appointments WITH (UPDLOCK,HOLDLOCK) WHERE DoctorId=@DoctorId AND AppointmentDate=@AppointmentDate AND AppointmentStatus NOT IN (N'Cancelled',N'No Show') AND StartTime<@EndTime AND EndTime>@StartTime) THEN 0 ELSE 1 END AS bit) IsAvailable; END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_Book @AppointmentCode NVARCHAR(40),@PatientId INT,@DoctorId INT,@DepartmentId INT,@AppointmentDate DATE,@StartTime TIME,@EndTime TIME,@ReasonForVisit NVARCHAR(1000)=NULL,@BookingSource NVARCHAR(30)=N'Patient',@ReceptionistId INT=NULL AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRY
  BEGIN TRANSACTION;
    IF @AppointmentDate<CONVERT(date,GETDATE()) OR @EndTime<=@StartTime THROW 51001,N'Appointment date or time is invalid.',1;
    IF NOT EXISTS(SELECT 1 FROM dbo.Patients WHERE PatientId=@PatientId AND IsActive=1) THROW 51002,N'Patient does not exist or is inactive.',1;
    SET DATEFIRST 1;
    DECLARE @ScheduleId INT,@Fee DECIMAL(10,2),@Day TINYINT=DATEPART(WEEKDAY,@AppointmentDate);
  SELECT TOP(1) @ScheduleId=ScheduleId FROM dbo.DoctorSchedules WITH (UPDLOCK,HOLDLOCK) WHERE DoctorId=@DoctorId AND DayOfWeek=@Day AND IsActive=1 AND @StartTime>=StartTime AND @EndTime<=EndTime AND DATEDIFF(MINUTE,@StartTime,@EndTime)=SlotDurationMinutes AND DATEDIFF(MINUTE,StartTime,@StartTime)%SlotDurationMinutes=0 AND (@StartTime>=ISNULL(BreakEndTime,'00:00') OR @EndTime<=ISNULL(BreakStartTime,'00:00'));
  SELECT @Fee=ConsultationFee FROM dbo.Doctors WHERE DoctorId=@DoctorId AND IsActive=1;
  IF @Fee IS NULL THROW 51003,N'Doctor does not exist or is inactive.',1;
  IF NOT EXISTS(SELECT 1 FROM dbo.Departments WHERE DepartmentId=@DepartmentId AND IsActive=1) THROW 51004,N'Department does not exist or is inactive.',1;
  IF NOT EXISTS(SELECT 1 FROM dbo.Doctors d JOIN dbo.Specializations s ON s.SpecializationId=d.SpecializationId WHERE d.DoctorId=@DoctorId AND s.DepartmentId=@DepartmentId) THROW 51011,N'The selected department does not belong to the doctor.',1;
  IF @ScheduleId IS NULL THROW 51005,N'The doctor does not work at the selected time.',1;
  IF EXISTS(SELECT 1 FROM dbo.Appointments WITH (UPDLOCK,HOLDLOCK) WHERE DoctorId=@DoctorId AND AppointmentDate=@AppointmentDate AND AppointmentStatus NOT IN (N'Cancelled',N'No Show') AND StartTime<@EndTime AND EndTime>@StartTime) THROW 51006,N'The selected time overlaps an existing appointment.',1;
  IF EXISTS(SELECT 1 FROM dbo.Appointments WHERE AppointmentCode=@AppointmentCode) THROW 51007,N'Appointment code already exists.',1;
  INSERT dbo.Appointments(AppointmentCode,PatientId,DoctorId,DepartmentId,AppointmentDate,StartTime,EndTime,ReasonForVisit,BookingSource,ConsultationFee,ReceptionistId) VALUES(@AppointmentCode,@PatientId,@DoctorId,@DepartmentId,@AppointmentDate,@StartTime,@EndTime,@ReasonForVisit,@BookingSource,@Fee,@ReceptionistId);
  COMMIT; SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AppointmentId;
 END TRY BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_GetAvailableSlots @DoctorId INT,@AppointmentDate DATE AS
BEGIN SET NOCOUNT ON; SET DATEFIRST 1; DECLARE @Day TINYINT=DATEPART(WEEKDAY,@AppointmentDate); WITH slots AS (SELECT s.StartTime SlotStart,s.EndTime SlotEnd,s.BreakStartTime,s.BreakEndTime,s.SlotDurationMinutes FROM dbo.DoctorSchedules s WHERE s.DoctorId=@DoctorId AND s.DayOfWeek=@Day AND s.IsActive=1 UNION ALL SELECT DATEADD(MINUTE,SlotDurationMinutes,SlotStart),SlotEnd,BreakStartTime,BreakEndTime,SlotDurationMinutes FROM slots WHERE DATEADD(MINUTE,SlotDurationMinutes,SlotStart)<SlotEnd) SELECT SlotStart,CONVERT(time,DATEADD(MINUTE,SlotDurationMinutes,SlotStart)) SlotEnd FROM slots WHERE (BreakStartTime IS NULL OR SlotStart>=BreakEndTime OR DATEADD(MINUTE,SlotDurationMinutes,SlotStart)<=BreakStartTime) AND NOT EXISTS(SELECT 1 FROM dbo.Appointments a WHERE a.DoctorId=@DoctorId AND a.AppointmentDate=@AppointmentDate AND a.AppointmentStatus NOT IN (N'Cancelled',N'No Show') AND a.StartTime<DATEADD(MINUTE,SlotDurationMinutes,SlotStart) AND a.EndTime>SlotStart) OPTION (MAXRECURSION 1000); END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_GetById @AppointmentId BIGINT AS 
SELECT a.*,
       p.PatientCode,
       p.FirstName AS PatientFirstName,
       p.LastName AS PatientLastName,
       d.DoctorCode,
       d.FirstName AS DoctorFirstName,
       d.LastName AS DoctorLastName,
       dep.DepartmentName,
       s.SpecializationName 
FROM dbo.Appointments a 
JOIN dbo.Patients p ON p.PatientId=a.PatientId 
JOIN dbo.Doctors d ON d.DoctorId=a.DoctorId 
JOIN dbo.Departments dep ON dep.DepartmentId=a.DepartmentId 
LEFT JOIN dbo.Specializations s ON s.SpecializationId=d.SpecializationId 
WHERE a.AppointmentId=@AppointmentId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_UpdateStatus @AppointmentId BIGINT,@AppointmentStatus NVARCHAR(30) AS UPDATE dbo.Appointments SET AppointmentStatus=@AppointmentStatus,UpdatedAt=SYSDATETIME() WHERE AppointmentId=@AppointmentId; IF @@ROWCOUNT=0 THROW 51008,N'Appointment not found.',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_Cancel @AppointmentId BIGINT AS EXEC dbo.usp_Appointment_UpdateStatus @AppointmentId,N'Cancelled';
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_Reschedule 
    @AppointmentId BIGINT,
    @AppointmentDate DATE,
    @StartTime TIME,
    @EndTime TIME 
AS 
BEGIN 
  SET NOCOUNT ON; 
  SET XACT_ABORT ON;
  DECLARE @p INT,@d INT,@dep INT,@code NVARCHAR(40),@reason NVARCHAR(1000),@source NVARCHAR(30),@rec INT, @status NVARCHAR(30); 
  SELECT @p=PatientId,@d=DoctorId,@dep=DepartmentId,@code=AppointmentCode,@reason=ReasonForVisit,@source=BookingSource,@rec=ReceptionistId, @status=AppointmentStatus 
  FROM dbo.Appointments WHERE AppointmentId=@AppointmentId; 
  
  IF @p IS NULL THROW 51009,N'Appointment not found.',1; 
  IF @status IN (N'Cancelled', N'Completed', N'No Show') THROW 51010,N'Cannot reschedule a cancelled or completed appointment.',1;

  DECLARE @newCode NVARCHAR(40) = LEFT(N'APT-' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N''), 40);

  BEGIN TRY
    BEGIN TRANSACTION;
      UPDATE dbo.Appointments SET AppointmentStatus=N'Cancelled',UpdatedAt=SYSDATETIME() WHERE AppointmentId=@AppointmentId; 
      EXEC dbo.usp_Appointment_Book 
          @AppointmentCode=@newCode,
          @PatientId=@p,
          @DoctorId=@d,
          @DepartmentId=@dep,
          @AppointmentDate=@AppointmentDate,
          @StartTime=@StartTime,
          @EndTime=@EndTime,
          @ReasonForVisit=@reason,
          @BookingSource=@source,
          @ReceptionistId=@rec; 
    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
  END CATCH
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_GetByPatient @PatientId INT AS 
SELECT a.*, 
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName, 
       dep.DepartmentName, 
       s.SpecializationName 
FROM dbo.Appointments a 
JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId 
JOIN dbo.Departments dep ON dep.DepartmentId = a.DepartmentId 
LEFT JOIN dbo.Specializations s ON s.SpecializationId = d.SpecializationId 
WHERE a.PatientId = @PatientId 
ORDER BY a.AppointmentDate DESC, a.StartTime DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_GetByDoctor @DoctorId INT,@FromDate DATE=NULL,@ToDate DATE=NULL AS EXEC dbo.usp_Doctor_GetAppointments @DoctorId,@FromDate,@ToDate;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Appointment_GetAll @FromDate DATE=NULL,@ToDate DATE=NULL AS SELECT * FROM dbo.Appointments WHERE (@FromDate IS NULL OR AppointmentDate>=@FromDate) AND (@ToDate IS NULL OR AppointmentDate<=@ToDate) ORDER BY AppointmentDate,StartTime;
GO
