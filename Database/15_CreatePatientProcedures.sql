USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetById @PatientId INT AS SELECT * FROM dbo.Patients WHERE PatientId=@PatientId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetProfile @UserId INT AS SELECT p.* FROM dbo.Patients p WHERE p.UserId=@UserId AND p.IsActive=1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_UpdateProfile @PatientId INT,@FirstName NVARCHAR(100),@LastName NVARCHAR(100)=NULL,@Gender NVARCHAR(20),@DateOfBirth DATE,@BloodGroup NVARCHAR(10)=NULL,@Phone NVARCHAR(20),@Email NVARCHAR(150)=NULL,@Address NVARCHAR(500)=NULL,@EmergencyContactName NVARCHAR(150)=NULL,@EmergencyContactPhone NVARCHAR(20)=NULL,@Allergies NVARCHAR(1000)=NULL,@MedicalHistory NVARCHAR(2000)=NULL AS
BEGIN SET NOCOUNT ON; UPDATE dbo.Patients SET FirstName=@FirstName,LastName=@LastName,Gender=@Gender,DateOfBirth=@DateOfBirth,BloodGroup=@BloodGroup,Phone=@Phone,Email=@Email,Address=@Address,EmergencyContactName=@EmergencyContactName,EmergencyContactPhone=@EmergencyContactPhone,Allergies=@Allergies,MedicalHistory=@MedicalHistory,UpdatedAt=SYSDATETIME() WHERE PatientId=@PatientId AND IsActive=1; IF @@ROWCOUNT=0 THROW 50002,N'Patient not found or inactive.',1; END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_Search @Search NVARCHAR(200)=NULL AS SELECT * FROM dbo.Patients WHERE IsActive=1 AND (@Search IS NULL OR PatientCode LIKE N'%'+@Search+N'%' OR FirstName LIKE N'%'+@Search+N'%' OR LastName LIKE N'%'+@Search+N'%' OR Phone LIKE N'%'+@Search+N'%');
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetDashboard @PatientId INT AS SELECT p.PatientId,p.PatientCode,p.FirstName,p.LastName,(SELECT COUNT(*) FROM dbo.Appointments a WHERE a.PatientId=p.PatientId) AppointmentCount,(SELECT COUNT(*) FROM dbo.Bills b WHERE b.PatientId=p.PatientId AND b.PaymentStatus<>N'Paid') OutstandingBills FROM dbo.Patients p WHERE p.PatientId=@PatientId;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetAppointments @PatientId INT AS SELECT a.*,d.FirstName DoctorFirstName,d.LastName DoctorLastName,dep.DepartmentName FROM dbo.Appointments a JOIN dbo.Doctors d ON d.DoctorId=a.DoctorId JOIN dbo.Departments dep ON dep.DepartmentId=a.DepartmentId WHERE a.PatientId=@PatientId ORDER BY a.AppointmentDate DESC,a.StartTime DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetMedicalHistory @PatientId INT AS SELECT mr.*,d.FirstName DoctorFirstName,d.LastName DoctorLastName FROM dbo.MedicalRecords mr JOIN dbo.Doctors d ON d.DoctorId=mr.DoctorId WHERE mr.PatientId=@PatientId ORDER BY mr.RecordDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetPrescriptions @PatientId INT AS SELECT p.*,i.MedicineName,i.Dosage,i.Frequency,i.Duration,i.Instructions FROM dbo.Prescriptions p LEFT JOIN dbo.PrescriptionItems i ON i.PrescriptionId=p.PrescriptionId WHERE p.PatientId=@PatientId ORDER BY p.PrescriptionDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetMedicalTests @PatientId INT AS SELECT t.*,r.ReportId,r.ReportTitle,r.ResultSummary,r.ReportFilePath FROM dbo.MedicalTests t LEFT JOIN dbo.MedicalTestReports r ON r.MedicalTestId=t.MedicalTestId WHERE t.PatientId=@PatientId ORDER BY t.TestDate DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Patient_GetBills @PatientId INT AS SELECT * FROM dbo.Bills WHERE PatientId=@PatientId ORDER BY BillDate DESC;
GO
