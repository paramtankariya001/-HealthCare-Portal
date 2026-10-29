USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_GetDashboard AS SELECT (SELECT COUNT(*) FROM dbo.Patients WHERE IsActive=1) ActivePatients,(SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentDate=CONVERT(date,GETDATE())) TodayAppointments,(SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentStatus=N'Pending') PendingAppointments,(SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentDate=CONVERT(date,GETDATE()) AND AppointmentStatus=N'Checked-In') CheckedInAppointments;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_GetPatients @Search NVARCHAR(200)=NULL AS EXEC dbo.usp_Patient_Search @Search;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_CreatePatient @PatientCode NVARCHAR(30),@FirstName NVARCHAR(100),@LastName NVARCHAR(100)=NULL,@Gender NVARCHAR(20),@DateOfBirth DATE,@BloodGroup NVARCHAR(10)=NULL,@Phone NVARCHAR(20),@Email NVARCHAR(150)=NULL,@Address NVARCHAR(500)=NULL AS INSERT dbo.Patients(PatientCode,FirstName,LastName,Gender,DateOfBirth,BloodGroup,Phone,Email,Address) VALUES(@PatientCode,@FirstName,@LastName,@Gender,@DateOfBirth,@BloodGroup,@Phone,@Email,@Address); SELECT SCOPE_IDENTITY() PatientId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_UpdatePatient @PatientId INT,@FirstName NVARCHAR(100),@LastName NVARCHAR(100)=NULL,@Gender NVARCHAR(20),@DateOfBirth DATE,@BloodGroup NVARCHAR(10)=NULL,@Phone NVARCHAR(20),@Email NVARCHAR(150)=NULL,@Address NVARCHAR(500)=NULL AS EXEC dbo.usp_Patient_UpdateProfile @PatientId,@FirstName,@LastName,@Gender,@DateOfBirth,@BloodGroup,@Phone,@Email,@Address;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_GetAppointments @Date DATE=NULL AS 
SELECT a.*,
       p.PatientCode,
       p.FirstName AS PatientFirstName,
       p.LastName AS PatientLastName,
       p.Phone AS PatientPhone,
       d.DoctorCode,
       d.FirstName AS DoctorFirstName,
       d.LastName AS DoctorLastName,
       dep.DepartmentName 
FROM dbo.Appointments a 
JOIN dbo.Patients p ON p.PatientId=a.PatientId 
JOIN dbo.Doctors d ON d.DoctorId=a.DoctorId 
JOIN dbo.Departments dep ON dep.DepartmentId=a.DepartmentId 
WHERE @Date IS NULL OR a.AppointmentDate=@Date 
ORDER BY a.AppointmentDate,a.StartTime;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_BookAppointment @AppointmentCode NVARCHAR(40),@PatientId INT,@DoctorId INT,@DepartmentId INT,@AppointmentDate DATE,@StartTime TIME,@EndTime TIME,@ReasonForVisit NVARCHAR(1000)=NULL,@ReceptionistId INT=NULL AS EXEC dbo.usp_Appointment_Book @AppointmentCode,@PatientId,@DoctorId,@DepartmentId,@AppointmentDate,@StartTime,@EndTime,@ReasonForVisit,N'Receptionist',@ReceptionistId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_CheckInPatient @AppointmentId BIGINT AS EXEC dbo.usp_Appointment_UpdateStatus @AppointmentId,N'Checked-In';
GO
CREATE OR ALTER PROCEDURE dbo.usp_Receptionist_GetDoctorAvailability @DoctorId INT,@AppointmentDate DATE AS EXEC dbo.usp_Appointment_GetAvailableSlots @DoctorId,@AppointmentDate;
GO
