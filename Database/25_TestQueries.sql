USE HealthCarePortalDB;
GO
SELECT DB_NAME() DatabaseName;
EXEC dbo.usp_User_Login @Email=N'admin@healthcare.local',@PasswordHash=N'DEV_ONLY_REPLACE_WITH_ASPNET_PASSWORD_HASH';
EXEC dbo.usp_Doctor_GetAll;
EXEC dbo.usp_Doctor_Search @Search=N'Neel';
DECLARE @DoctorId INT=(SELECT DoctorId FROM dbo.Doctors WHERE DoctorCode=N'DR0001');
DECLARE @TestDate DATE=DATEADD(DAY,14,CONVERT(date,GETDATE()));
EXEC dbo.usp_Appointment_GetAvailableSlots @DoctorId,@TestDate;
-- Use a future Monday or Wednesday matching the seeded schedules for a successful booking.
-- EXEC dbo.usp_Appointment_Book N'APT-TEST-001',1,@DoctorId,1,@TestDate,'12:00','12:30',N'Test booking',N'Patient',NULL;
-- EXEC dbo.usp_Appointment_Book N'APT-TEST-002',1,@DoctorId,1,@TestDate,'12:15','12:45',N'Expected overlap failure',N'Patient',NULL;
EXEC dbo.usp_Appointment_GetByPatient @PatientId=1;
EXEC dbo.usp_Appointment_GetByDoctor @DoctorId=@DoctorId;
EXEC dbo.usp_Appointment_GetAll;
-- EXEC dbo.usp_Appointment_UpdateStatus @AppointmentId=1,@AppointmentStatus=N'Confirmed';
-- EXEC dbo.usp_Consultation_Create @AppointmentId=5,@Diagnosis=N'Routine follow-up';
-- Prescription TVP example requires a declared dbo.PrescriptionItemTableType variable.
-- Bill/payment TVP examples are shown in the procedure definitions and should be called from the repository with structured parameters.
EXEC dbo.usp_MedicalTest_GetByPatient @PatientId=1;
EXEC dbo.usp_MedicalTest_GetReport @MedicalTestId=3;
EXEC dbo.usp_Bill_GetByPatient @PatientId=1;
EXEC dbo.usp_Payment_GetByPatient @PatientId=1;
EXEC dbo.usp_Patient_GetDashboard @PatientId=1;
EXEC dbo.usp_Doctor_GetDashboard @DoctorId=@DoctorId;
EXEC dbo.usp_Admin_GetDashboard;
DECLARE @ReportFromDate DATE=DATEADD(DAY,-30,CONVERT(date,GETDATE()));
EXEC dbo.usp_Report_RevenueSummary @FromDate=@ReportFromDate;
EXEC dbo.usp_Report_AppointmentSummary;
EXEC dbo.usp_AuditLog_GetAll;
GO
