USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Consultation_Create @AppointmentId BIGINT,@Symptoms NVARCHAR(2000)=NULL,@BloodPressure NVARCHAR(20)=NULL,@Temperature DECIMAL(5,2)=NULL,@Weight DECIMAL(6,2)=NULL,@PulseRate INT=NULL,@Diagnosis NVARCHAR(2000)=NULL,@DoctorNotes NVARCHAR(4000)=NULL,@FollowUpDate DATE=NULL AS
BEGIN SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRY BEGIN TRANSACTION; DECLARE @PatientId INT,@DoctorId INT; SELECT @PatientId=PatientId,@DoctorId=DoctorId FROM dbo.Appointments WHERE AppointmentId=@AppointmentId; IF @PatientId IS NULL THROW 52001,N'Appointment not found.',1; IF EXISTS(SELECT 1 FROM dbo.Consultations WHERE AppointmentId=@AppointmentId) THROW 52002,N'Appointment already has a consultation.',1; INSERT dbo.Consultations(AppointmentId,PatientId,DoctorId,Symptoms,BloodPressure,Temperature,Weight,PulseRate,Diagnosis,DoctorNotes,FollowUpDate) VALUES(@AppointmentId,@PatientId,@DoctorId,@Symptoms,@BloodPressure,@Temperature,@Weight,@PulseRate,@Diagnosis,@DoctorNotes,@FollowUpDate); UPDATE dbo.Appointments SET AppointmentStatus=N'Completed',UpdatedAt=SYSDATETIME() WHERE AppointmentId=@AppointmentId; COMMIT; SELECT CAST(SCOPE_IDENTITY() AS BIGINT) ConsultationId; END TRY BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Consultation_GetByAppointment @AppointmentId BIGINT AS 
SELECT c.*, 
       p.PatientCode, 
       p.FirstName AS PatientFirstName, 
       p.LastName AS PatientLastName, 
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName 
FROM dbo.Consultations c 
JOIN dbo.Patients p ON p.PatientId = c.PatientId 
JOIN dbo.Doctors d ON d.DoctorId = c.DoctorId 
WHERE c.AppointmentId = @AppointmentId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Consultation_GetByPatient @PatientId INT AS 
SELECT c.*, 
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName,
       dep.DepartmentName 
FROM dbo.Consultations c 
JOIN dbo.Doctors d ON d.DoctorId = c.DoctorId 
LEFT JOIN dbo.Specializations s ON s.SpecializationId = d.SpecializationId 
LEFT JOIN dbo.Departments dep ON dep.DepartmentId = s.DepartmentId 
WHERE c.PatientId = @PatientId 
ORDER BY c.ConsultationDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Consultation_GetByDoctor @DoctorId INT AS SELECT * FROM dbo.Consultations WHERE DoctorId=@DoctorId ORDER BY ConsultationDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalRecord_Create @PatientId INT,@DoctorId INT,@AppointmentId BIGINT=NULL,@ConsultationId BIGINT=NULL,@RecordType NVARCHAR(50),@RecordTitle NVARCHAR(200)=NULL,@Description NVARCHAR(4000)=NULL,@RecordDate DATE AS INSERT dbo.MedicalRecords(PatientId,DoctorId,AppointmentId,ConsultationId,RecordType,RecordTitle,Description,RecordDate) VALUES(@PatientId,@DoctorId,@AppointmentId,@ConsultationId,@RecordType,@RecordTitle,@Description,@RecordDate); SELECT CAST(SCOPE_IDENTITY() AS BIGINT) MedicalRecordId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalRecord_GetByPatient @PatientId INT AS SELECT * FROM dbo.MedicalRecords WHERE PatientId=@PatientId ORDER BY RecordDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalRecord_GetById @MedicalRecordId BIGINT AS SELECT * FROM dbo.MedicalRecords WHERE MedicalRecordId=@MedicalRecordId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Prescription_Create @PrescriptionCode NVARCHAR(40),@PatientId INT,@DoctorId INT,@Items dbo.PrescriptionItemTableType READONLY,@AppointmentId BIGINT=NULL,@ConsultationId BIGINT=NULL,@Diagnosis NVARCHAR(2000)=NULL,@Instructions NVARCHAR(3000)=NULL,@FollowUpDate DATE=NULL AS
BEGIN SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRY BEGIN TRANSACTION; IF NOT EXISTS(SELECT 1 FROM dbo.Patients WHERE PatientId=@PatientId AND IsActive=1) THROW 52003,N'Patient not found.',1; IF NOT EXISTS(SELECT 1 FROM @Items) THROW 52004,N'At least one prescription item is required.',1; INSERT dbo.Prescriptions(PrescriptionCode,PatientId,DoctorId,AppointmentId,ConsultationId,Diagnosis,Instructions,FollowUpDate) VALUES(@PrescriptionCode,@PatientId,@DoctorId,@AppointmentId,@ConsultationId,@Diagnosis,@Instructions,@FollowUpDate); DECLARE @PrescriptionId BIGINT=CONVERT(bigint,SCOPE_IDENTITY()); INSERT dbo.PrescriptionItems(PrescriptionId,MedicineName,Dosage,Frequency,Duration,Instructions) SELECT @PrescriptionId,MedicineName,Dosage,Frequency,Duration,Instructions FROM @Items; COMMIT; SELECT @PrescriptionId PrescriptionId; END TRY BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Prescription_GetById @PrescriptionId BIGINT AS SELECT p.*,i.PrescriptionItemId,i.MedicineName,i.Dosage,i.Frequency,i.Duration,i.Instructions FROM dbo.Prescriptions p LEFT JOIN dbo.PrescriptionItems i ON i.PrescriptionId=p.PrescriptionId WHERE p.PrescriptionId=@PrescriptionId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Prescription_GetByPatient @PatientId INT AS 
SELECT p.*, 
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName 
FROM dbo.Prescriptions p 
JOIN dbo.Doctors d ON d.DoctorId = p.DoctorId 
WHERE p.PatientId = @PatientId 
ORDER BY p.PrescriptionDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Prescription_GetByDoctor @DoctorId INT AS 
SELECT p.*, 
       pt.PatientCode, 
       pt.FirstName AS PatientFirstName, 
       pt.LastName AS PatientLastName 
FROM dbo.Prescriptions p 
JOIN dbo.Patients pt ON pt.PatientId = p.PatientId 
WHERE p.DoctorId = @DoctorId 
ORDER BY p.PrescriptionDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Prescription_GetHistory @PatientId INT AS EXEC dbo.usp_Prescription_GetByPatient @PatientId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_Create @TestCode NVARCHAR(40),@PatientId INT,@DoctorId INT,@AppointmentId BIGINT=NULL,@TestName NVARCHAR(200),@Priority NVARCHAR(30)=N'Normal',@Instructions NVARCHAR(1000)=NULL,@TestDate DATE AS INSERT dbo.MedicalTests(TestCode,PatientId,DoctorId,AppointmentId,TestName,Priority,Instructions,TestDate) VALUES(@TestCode,@PatientId,@DoctorId,@AppointmentId,@TestName,@Priority,@Instructions,@TestDate); SELECT CAST(SCOPE_IDENTITY() AS BIGINT) MedicalTestId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_GetById @MedicalTestId BIGINT AS 
SELECT t.*, 
       p.PatientCode, 
       p.FirstName AS PatientFirstName, 
       p.LastName AS PatientLastName,
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName,
       r.ReportId, 
       r.ReportTitle, 
       r.ResultSummary, 
       r.ReportFilePath, 
       r.ReportedBy, 
       r.ReportDate 
FROM dbo.MedicalTests t 
JOIN dbo.Patients p ON p.PatientId = t.PatientId 
JOIN dbo.Doctors d ON d.DoctorId = t.DoctorId 
LEFT JOIN dbo.MedicalTestReports r ON r.MedicalTestId = t.MedicalTestId 
WHERE t.MedicalTestId = @MedicalTestId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_GetByPatient @PatientId INT AS 
SELECT t.*, 
       d.DoctorCode, 
       d.FirstName AS DoctorFirstName, 
       d.LastName AS DoctorLastName,
       r.ReportId, 
       r.ReportTitle, 
       r.ResultSummary,
       r.ReportFilePath 
FROM dbo.MedicalTests t 
JOIN dbo.Doctors d ON d.DoctorId = t.DoctorId 
LEFT JOIN dbo.MedicalTestReports r ON r.MedicalTestId = t.MedicalTestId 
WHERE t.PatientId = @PatientId 
ORDER BY t.TestDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_GetByDoctor @DoctorId INT AS 
SELECT t.*, 
       p.PatientCode, 
       p.FirstName AS PatientFirstName, 
       p.LastName AS PatientLastName,
       r.ReportId, 
       r.ReportTitle, 
       r.ResultSummary 
FROM dbo.MedicalTests t 
JOIN dbo.Patients p ON p.PatientId = t.PatientId 
LEFT JOIN dbo.MedicalTestReports r ON r.MedicalTestId = t.MedicalTestId 
WHERE t.DoctorId = @DoctorId 
ORDER BY t.TestDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_UpdateStatus @MedicalTestId BIGINT,@TestStatus NVARCHAR(40) AS UPDATE dbo.MedicalTests SET TestStatus=@TestStatus,UpdatedAt=SYSDATETIME() WHERE MedicalTestId=@MedicalTestId; IF @@ROWCOUNT=0 THROW 52005,N'Medical test not found.',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_CreateReport @MedicalTestId BIGINT,@ReportTitle NVARCHAR(200)=NULL,@ResultSummary NVARCHAR(4000)=NULL,@ReportFilePath NVARCHAR(500)=NULL,@ReportedBy NVARCHAR(200)=NULL,@ReportDate DATE=NULL AS BEGIN SET NOCOUNT ON; INSERT dbo.MedicalTestReports(MedicalTestId,ReportTitle,ResultSummary,ReportFilePath,ReportedBy,ReportDate) VALUES(@MedicalTestId,@ReportTitle,@ResultSummary,@ReportFilePath,@ReportedBy,@ReportDate); UPDATE dbo.MedicalTests SET TestStatus=N'Report Available' WHERE MedicalTestId=@MedicalTestId; END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_MedicalTest_GetReport @MedicalTestId BIGINT AS SELECT r.* FROM dbo.MedicalTestReports r WHERE r.MedicalTestId=@MedicalTestId;
GO
